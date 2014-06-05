using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ClipboardHelper.Win32;

namespace ClipboardHelper
{
    public class Clipboard : IDisposable
    {
        private bool disposed;

        private static readonly IntPtr notOwned = new IntPtr(-1);

        private IntPtr ClipboardOwner;
        public Clipboard()
        {
            ClipboardOwner=new IntPtr(-1);
            var formats=Enum.GetValues(typeof (StandartClipboardFormats));
            foreach (uint format in formats)
            {
                registeredFormats.Add(((StandartClipboardFormats)format).ToString(), format);
            }
        }

        public void Open()
        {
            this.Open(IntPtr.Zero);
        }

        public void Open(IntPtr hWnd)
        {
            if (ClipboardOwner != notOwned)
                throw new ClipboardOpenedException("Clipboard allready opened");

            var opened = OpenClipboard(hWnd);
            if (!opened)
            {
                var errcode = Marshal.GetLastWin32Error();
                var innerException=Marshal.GetExceptionForHR(errcode);
                throw new OpenClipboardException(new System.ComponentModel.Win32Exception());
            }
            ClipboardOwner = hWnd;
        }

        public void Clear()
        {
            GuardClipbordOpened();
            if (!EmptyClipboard())
            {
                throw new ClipboardException("Error clearing clipboard", ExceptionHelpers.GetLastWin32Exception());
            }
        }

        private void GuardClipbordOpened(bool checkWindowHandle=false)
        {
            if (ClipboardOwner == notOwned)
                throw new ClipboardClosedException("Cannot perform operation on closed clipbord");
            if (checkWindowHandle && ClipboardOwner == IntPtr.Zero)
                throw new ClipboardClosedException("This operation reqare clipboard to be opened with set hwnd.");
        }


        public T GetData<T>(IClipbordFormatProvider<T> provider)
        {
            var formatId = GetFormatId(provider.FormatId);
            if (!IsDataAvailable(provider))
                throw new ClipboardDataException("No data avalible in selected format", ExceptionHelpers.GetLastWin32Exception());

            GuardClipbordOpened();

            IntPtr memHandle = GetClipboardData(formatId);
            if (memHandle == IntPtr.Zero)
                throw new ClipboardDataException("Error receiving clipbord data", ExceptionHelpers.GetLastWin32Exception());

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
                throw new ClipboardDataException("Error receiving data from clipbord",exception);
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
                throw new ClipboardDataException("Error saveing data to clipbord", exception);
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

        public uint SequenceNumber
        {
            get { return GetClipboardSequenceNumber(); }
        }

        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

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


#if WINNT_VISTA

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetUpdatedClipboardFormats(ref uint[] lpuiFormats, uint cFormats, [Out] UIntPtr pcFormatsOut);
#endif

        public void RegisterFormatProvider<T>(IClipbordFormatProvider<T> provider)
        {

        }
        public List<uint> GetAvalibleFromats()
        {
            GuardClipbordOpened();
            List<uint> formats= new List<uint>();
            uint currentFormat = 0;
            while (true)
            {
                currentFormat = EnumClipboardFormats(currentFormat);
                if (currentFormat != 0)
                    formats.Add(currentFormat);
                else
                {
                    var err = Marshal.GetLastWin32Error();
                    if (err == 0)
                        return formats;
                    throw new ClipboardDataException("Error in retreiving avalible clipbord formats",
                        ExceptionHelpers.GetLastWin32Exception());

                }
            }
        }



        public void Close()
        {
            if (ClipboardOwner == notOwned)
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
            if (ClipboardOwner != notOwned)
                CloseClipboard();
            ClipboardOwner = new IntPtr(-1);
        }
    }

    public class ClipboardListener
    {
#if WINNT_VISTA

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        
#endif
    }
}
