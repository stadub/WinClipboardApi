using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardHelper
{
    public class DataFormat
    {
        List<IClipbordFormatProvider> providers= new List<IClipbordFormatProvider>();

        protected void AddProvider<T>(DataFormatProvider<T> provider)
        {
            providers.Add(provider);
        }
    }
}
