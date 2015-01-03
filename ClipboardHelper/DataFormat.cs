using System.Collections.Generic;
using ClipboardHelper.FormatProviders;

namespace ClipboardHelper
{
    public static class Providers{
        public static IEnumerable<IClipbordFormatProvider> GetCurrentProviders(){
            yield return new FileDropProvider();
            yield return new UnicodeFileNameProvider();
            yield return new HtmlFormatProvider();
            yield return new SkypeFormatProvider();
            yield return new UnicodeTextProvider();
        }
    }
    public class DataFormat
    {
        List<IClipbordFormatProvider> providers= new List<IClipbordFormatProvider>();

        protected void AddProvider<T>(IClipbordFormatProvider<T> provider)
        {
            providers.Add(provider);
        }

    }

}
