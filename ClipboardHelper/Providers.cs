using System;
using System.Collections.Generic;
using ClipboardHelper.FormatProviders;

namespace ClipboardHelper
{
    public static class Providers{
        public static IEnumerable<Func<IClipbordFormatProvider>> GetCurrentProviders()
        {
            yield return ()=>new FileDropProvider();
            yield return ()=>new UnicodeFileNameProvider();
            yield return ()=>new HtmlFormatProvider();
            yield return ()=>new SkypeFormatProvider();
            yield return ()=>new UnicodeTextProvider();
        }
    }
}
