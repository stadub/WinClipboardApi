using System;
using ClipboardHelper.FormatProviders;
using ClipboardHelper.WinApi;

namespace ClipboardHelper
{
    public class ClipboardWriter : IClipboardWriter
    {
        private Clipboard clipboard;

        public ClipboardWriter(Clipboard clipboard)
        {
            this.clipboard = clipboard;
        }

        public void SetData(IClipbordFormatProvider provider)
        {
            clipboard.GuardClipbordOpened(true);
            clipboard.SetDataInt(provider);
        }

        public void EnrolDataFormat(IClipbordFormatProvider provider)
        {
            var formatId = clipboard.GetFormatId(provider.FormatId);

            clipboard.GuardClipbordOpened(true);
            try
            {
                ClipbordWinApi.SetClipboardData(formatId, IntPtr.Zero);
            }
            catch (GlobalMemoryException exception)
            {
                throw new ClipboardDataException("Can't save data to clipbord", exception);
            }
        }

        public void Clear()
        {
            clipboard.Clear();
        }

        public void Close()
        {
            clipboard.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}