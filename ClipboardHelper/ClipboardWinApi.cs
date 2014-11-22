using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ClipboardHelper.FormatProviders;
using ClipboardHelper.Win32;

namespace ClipboardHelper
{
    public class Clipboard : IDisposable
    {
        private bool disposed;

        private bool Owned;

        private IntPtr ClipboardOwner;

        protected Clipboard(IntPtr ownerHwnd)
        {
            var formats = Enum.GetValues(typeof(StandartClipboardFormats));
            foreach (StandartClipboardFormats format in formats)
            {
                var formatIdWraper = new StandardFormatIdWraper(format);

                registeredFormats.Add(formatIdWraper.FormatName, formatIdWraper.FormatId);
            }

            ClipboardOwner = ownerHwnd;
        }

        protected Clipboard()
            : this(IntPtr.Zero)
        {
        }

        public static Clipboard CreateReadOnly()
        {
            return new Clipboard();
        }

        public static Clipboard CreateReadWrite(IntPtr ownerHwnd)
        {
            return new Clipboard(ownerHwnd);
        }

        public void OpenReadOnly()
        {
            OpenInt();
        }

        public void Open()
        {
            if (ClipboardOwner == IntPtr.Zero)
                throw new OpenClipboardException("Empty window handle is allowed only for Read only mode.To be able to write to clipboard Clipboard(IntPtr ownerHwnd) constructor should be used.");
            OpenInt();
        }

        protected void OpenInt()
        {
            if (Owned)
                throw new ClipboardOpenedException("Clipboard allready opened");
            Owned = true;
            var opened = OpenClipboard(ClipboardOwner);
            if (!opened)
            {
                var errcode = Marshal.GetLastWin32Error();
                var innerException = Marshal.GetExceptionForHR(errcode);
                Owned = false;
                throw new OpenClipboardException(new Win32Exception());
            }
        }

        public void Clear()
        {
            GuardClipbordOpened();
            if (!EmptyClipboard())
            {
                throw new ClipboardException("Can't clear clipboard content", ExceptionHelpers.GetLastWin32Exception());
            }
        }

        private void GuardClipbordOpened(bool checkWindowHandle=false)
        {
            if (!Owned)
                throw new ClipboardClosedException("Operation cannot be performed on the closed clipbord");
            if (checkWindowHandle && ClipboardOwner == IntPtr.Zero)
                throw new ClipboardClosedException("This operation requare clipboard oened in Write mode.");
        }


        public T GetData<T>(IClipbordFormatProvider<T> provider)
        {
            var formatId = GetFormatId(provider.FormatId);
            if (!IsDataAvailable(provider))
                throw new ClipboardDataException("There no data of selected format in the Clipboard", ExceptionHelpers.GetLastWin32Exception());

            GuardClipbordOpened();

            IntPtr memHandle = GetClipboardData(formatId);
            if (memHandle == IntPtr.Zero)
                throw new ClipboardDataException("Can't receive data from clipbord", ExceptionHelpers.GetLastWin32Exception());

            try
            {
                using (var memory = new GlobalMemory(memHandle))
                {
                    int lenght = (int)memory.Size();
                    var memPtr = memory.Lock();
                    var buffer = new byte[lenght];
                    Marshal.Copy(memPtr, buffer, 0, lenght);

                    return provider.Deserialize(buffer);
                }
            }
            catch (GlobalMemoryException exception)
            {
                throw new ClipboardDataException("Can't receive data from clipbord",exception);
            }
        }

        public bool IsDataAvailable<T>(IClipbordFormatProvider<T> provider)
        {
            var formatId = GetFormatId(provider.FormatId);
            return IsClipboardFormatAvailable(formatId);
        }

        List<GlobalMemory> allocatedFormats= new List<GlobalMemory>();
        public void SetData<T>(T data,IClipbordFormatProvider<T> provider)
        {
            var formatId = GetFormatId(provider.FormatId);
            var bytes=provider.Serialize(data);

            GuardClipbordOpened(true);

            int size = bytes.Length;
            var memory = new GlobalMemory();
            allocatedFormats.Add(memory);
            try
            {
                var handle = memory.Alloc(size+1, GlobalMemoryFlags.GmemMoveable);
                var memPtr=memory.Lock();
                
                Marshal.Copy(bytes, 0, memPtr, size);
                memory.Unlock();

                SetClipboardData(formatId, handle);
            }
            catch (GlobalMemoryException exception)
            {
                throw new ClipboardDataException("Can't save data to clipbord", exception);
            }
        }

        private readonly Dictionary<string,uint> registeredFormats = new Dictionary<string,uint>();


        private uint GetFormatId(string formatId)
        {
            if (registeredFormats.ContainsKey(formatId))
                return registeredFormats[formatId];

            uint id = RegisterClipboardFormat(formatId);
            registeredFormats.Add(formatId, id);
            return id;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RegisterClipboardFormat(string lpszFormat);

        [DllImport("user32.dll", ThrowOnUnmappableChar = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();


        [DllImport("user32.dll",SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll")]
        public static extern uint EnumClipboardFormats(uint format);

        [DllImport("user32.dll", ThrowOnUnmappableChar = true)]
        private static extern int GetClipboardFormatName(uint format,
            [Out][MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFormatName, int cchMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardOwner();

        [DllImport("user32.dll")]
        private static extern int GetPriorityClipboardFormat(UIntPtr paFormatPriorityList, int cFormats);


#if WIN_VISTA

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetUpdatedClipboardFormats(ref uint[] lpuiFormats, uint cFormats, [Out] UIntPtr pcFormatsOut);
#endif


        public IEnumerable<IClipbordFormatProvider> GetAvalibleFromats(IEnumerable<IClipbordFormatProvider> formats,bool includeUnknown=false)
        {
            var fromatsDict=formats.ToDictionary(provider => provider.FormatId);
            GuardClipbordOpened();
            List<uint> unknownformatsIds= new List<uint>();
            uint currentFormat = 0;
            var registeredIds=registeredFormats.ToDictionary(pair => pair.Value);
            while (true)
            {
                currentFormat = EnumClipboardFormats(currentFormat);
                if (currentFormat != 0)
                {
                    KeyValuePair<string, uint> registeredFormatId;
                    if (registeredIds.TryGetValue(currentFormat, out registeredFormatId))
                    {
                        if (fromatsDict.ContainsKey(registeredFormatId.Key))
                            yield return fromatsDict[registeredFormatId.Key];
                        else
                            unknownformatsIds.Add(currentFormat);
                    }
                    else
                        unknownformatsIds.Add(currentFormat);
                }
                else
                {
                    var err = Marshal.GetLastWin32Error();
                    if (err == 0)
                    {
                        if (includeUnknown)
                        {
                            foreach (var unknownformatId in unknownformatsIds)
                                yield return new UnknownFormatProvider(unknownformatId);
                        }
                        yield break; 
                    }
                    throw new ClipboardDataException("Error in retreiving avalible clipbord formats",
                        ExceptionHelpers.GetLastWin32Exception());

                }
            }
        }



        public void Close()
        {
            if (!Owned)
                return;
            Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                }
                disposed = true;
            }
            if (Owned)
                CloseClipboard();
            ClipboardOwner = new IntPtr(-1);
        }
    }
}
