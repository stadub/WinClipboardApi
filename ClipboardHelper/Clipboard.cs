using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ClipboardHelper.FormatProviders;
using ClipboardHelper.Watcher;
using ClipboardHelper.WinApi;
using Utils;

namespace ClipboardHelper
{

    public class Clipboard : IClipboard
    {
        private bool disposed;

        private volatile bool Owned;

        private volatile IntPtr ClipboardOwner;

        private readonly Dictionary<string, uint> registeredFormats = new Dictionary<string, uint>();
        private Dictionary<string, Func<IClipbordFormatProvider>> formatProviders= new Dictionary<string, Func<IClipbordFormatProvider>>();


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

        public Clipboard()
            : this(IntPtr.Zero)
        {
        }


        public IClipboardWriter CreateWriter(IClipbordMessageProvider provider)
        {
            ClipboardOwner = provider.WindowHandle;
            OpenInt();
            return new ClipboardWriter(this);
        }

        public IClipboardReader CreateReader()
        {
            OpenInt();
            return new ClipboardReader(this,registeredFormats,formatProviders);
        }

        private void Open()
        {
            if (ClipboardOwner == IntPtr.Zero)
                throw new OpenClipboardException("Empty window handle is allowed  only for Read only mode.To be able to write to clipboard Clipboard(IntPtr ownerHwnd) constructor should be used.");
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

        internal void Clear()
        {
            GuardClipbordOpened();
            allocatedFormats.ForEach(x=>x.Dispose());
            allocatedFormats.Clear();
            if (!EmptyClipboard())
            {
                throw new ClipboardException("Can't clear clipboard content", ExceptionHelpers.GetLastWin32Exception());
            }
        }

        internal void GuardClipbordOpened(bool checkWindowHandle=false)
        {
            if (!Owned)
                throw new ClipboardClosedException("Operation cannot be performed on the closed clipbord");
            if (checkWindowHandle && ClipboardOwner == IntPtr.Zero)
                throw new ClipboardClosedException("This operation requare clipboard oened in Write mode.");
        }

        public void RegisterFormatProviders(IEnumerable<Func<IClipbordFormatProvider>> providers)
        {
            foreach (var provider in providers)
                RegisterFormatProvider(provider);
        }

        public void RegisterFormatProvider(Func<IClipbordFormatProvider> provider )
        {
            var formatId = provider().FormatId;

            var id = GetFormatId(formatId);
            formatProviders.Add(formatId, provider);
        }

        public bool IsDataAvailable(IClipbordFormatProvider provider)
        {
            var formatId = GetFormatId(provider.FormatId);
            return IsClipboardFormatAvailable(formatId);
        }

        List<GlobalMemory> allocatedFormats= new List<GlobalMemory>();



        public void SetRequestedData(IClipbordFormatProvider provider)
        {
            SetDataInt(provider);
        }

        protected internal void SetDataInt(IClipbordFormatProvider provider)
        {
            var formatId = GetFormatId(provider.FormatId);
            var bytes=provider.Serialize();

            int size = bytes.Length;
            var memory = new GlobalMemory();
            allocatedFormats.Add(memory);
            try
            {
                var handle = memory.Alloc(size+1, GlobalMemoryFlags.GmemMoveable);
                var memPtr=memory.Lock();
                
                Marshal.Copy(bytes, 0, memPtr, size);
                memory.Unlock();

                ClipbordWinApi.SetClipboardData(formatId, handle);
            }
            catch (GlobalMemoryException exception)
            {
                throw new ClipboardDataException("Can't save data to clipbord", exception);
            }
        }


        public uint GetFormatId(string formatId)
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


        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();



        [DllImport("user32.dll")]
        public static extern uint EnumClipboardFormats(uint format);


        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardOwner();

        [DllImport("user32.dll")]
        private static extern int GetPriorityClipboardFormat(UIntPtr paFormatPriorityList, int cFormats);


#if WIN_VISTA

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetUpdatedClipboardFormats(ref uint[] lpuiFormats, uint cFormats, [Out] UIntPtr pcFormatsOut);
#endif

        public void Close()
        {
            if (!Owned)
                return;
            if (Owned)
                CloseClipboard();
            Owned = false;
            ClipboardOwner = IntPtr.Zero;
        }

        public bool Closed
        {
            get { return Owned; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)return;
            disposed = true;
            if (disposing)
            {

            }
            Close(); 
        }
    }

    internal static class ClipbordWinApi
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll", ThrowOnUnmappableChar = true, CharSet = CharSet.Unicode)]
        public static extern int GetClipboardFormatName(uint format,
            [Out][MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFormatName, int cchMaxCount);


        [DllImport("user32.dll", ThrowOnUnmappableChar = true)]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
    }
}
