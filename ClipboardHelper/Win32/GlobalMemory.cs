using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using ClipboardHelper.WinApi;

namespace ClipboardHelper.Win32
{
    [Flags]
    enum GlobalMemoryFlags:uint
    {
        /// <summary>
        /// Combines GmemMoveable and GmemZeroinit.
        /// </summary>
        Ghnd=0x0042,
        /// <summary>
        /// Allocates fixed memory. The return value is a pointer.
        /// </summary>
        GmemFixed=0x0000,
        /// <summary>
        /// Allocates movable memory. Memory blocks are never moved in physical memory, but they can be moved within the default heap.
        /// The return value is a handle to the memory object. To translate the handle into a pointer, use the GlobalLock function.
        /// This value cannot be combined with GmemFixed.
        /// </summary>
        GmemMoveable=0x0002,
        /// <summary>
        /// Initializes memory contents to zero.
        /// </summary>
        GmemZeroinit=0x0040,
        /// <summary>
        /// Combines GmemFixed and GmemZeroinit.
        /// </summary>
        Gptr = 0x0040,
    }
        
    class GlobalMemory:IDisposable
    {
        private const int ERROR_NOT_LOCKED=158;
        private IntPtr hMem;
        private int locked;
        private bool allocated;

        public GlobalMemory(IntPtr hMem)
        {
            this.hMem = hMem;
        }

        public GlobalMemory()
        {

        }

        public IntPtr Alloc(int bytes, GlobalMemoryFlags flags)
        {
            if(bytes<=0)
                throw new GlobalMemoryException("Acllocation memory size cannot be less than zero");
            if(bytes<=0)
                throw new GlobalMemoryException("Allocating memory size should be non zerro value");

            if (hMem != IntPtr.Zero)
                throw new AlreadyHaveMemoryBlcokException("Allready associated memory block,for allocate mew memory block use new istance of the clsss");
            this.hMem = Memory.GlobalAlloc((uint)flags, new UIntPtr((uint)bytes));
            GuardZeroHandle(hMem,"Cannot allocate global memory");
            allocated = true;
            return hMem;
        }


        public uint Size()
        {
            GuardNotContainsValue();
            var size = Memory.GlobalSize(hMem);
            GuardZeroHandle(size,"Cannot determine memory block size");
            return size.ToUInt32();
        }
        public void Unlock()
        {
            int current=Interlocked.Decrement(ref locked);
            if (current < 0)
            {
                Interlocked.Exchange(ref locked, 0);
                return;
            }
            if (Memory.GlobalUnlock(hMem))
            {
                var err = Marshal.GetLastWin32Error();
                if(err==ERROR_NOT_LOCKED)
                    return;
                ThrowExceptionIfZero(0,"Cannot unlock memory block");
            }
        }
        public IntPtr Lock()
        {
            Interlocked.Increment(ref locked);
            IntPtr memPtr = Memory.GlobalLock(hMem);
            GuardZeroHandle(memPtr,"Cannot lock memory block");
            return memPtr;
        }

        private void GuardNotContainsValue()
        {
            if (hMem == IntPtr.Zero)
            {
                throw new NotHaveMemoryBlcokException();
            }
        }

        private static void GuardZeroHandle(UIntPtr ptr, string message)
        {
            ThrowExceptionIfZero((int)ptr.ToUInt32(), message);
        }

        private static void GuardZeroHandle(IntPtr ptr, string message)
        {
            ThrowExceptionIfZero(ptr.ToInt32(), message);
        }

        private static void ThrowExceptionIfZero(int ptr, string message)
        {
            if (ptr == 0)
                throw new GlobalMemoryException(message,new Win32Exception());
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (hMem != IntPtr.Zero)
            {
                while (locked>0)
                {
                    Unlock();
                }
            }
            if (allocated)
            {
                Memory.GlobalFree(hMem);
            }
            disposed = true;
        }

        ~GlobalMemory()
        {
            Dispose(false);
        }

        
    }
}
