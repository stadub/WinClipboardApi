using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipboardHelper.FormatProviders;

namespace ClipboardHelper
{
    public interface IClipboardReader : IDisposable
    {
        void GetData(IClipbordFormatProvider provider);
        IEnumerable<IClipbordFormatProvider> GetAvalibleFromats(bool includeUnknown = false);
        void Close();
    }

    public interface IClipboardWriter : IDisposable
    {
        void SetData(IClipbordFormatProvider provider);
        void EnrolDataFormat(IClipbordFormatProvider provider);
        void Clear();
        void Close();
    }

    public interface IClipboard : IDisposable
    {
        IClipboardReader CreateReader();

        void RegisterFormatProvider(Func<IClipbordFormatProvider> formatProvider);

        bool IsDataAvailable(IClipbordFormatProvider provider);
        void SetRequestedData(IClipbordFormatProvider provider);

        bool Closed { get; }

    }


}
