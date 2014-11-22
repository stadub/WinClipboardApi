using System.Collections.Generic;

namespace ClipboardHelper
{
    public class DataFormat
    {
        List<IClipbordFormatProvider> providers= new List<IClipbordFormatProvider>();

        protected void AddProvider<T>(IClipbordFormatProvider<T> provider)
        {
            providers.Add(provider);
        }
    }
}
