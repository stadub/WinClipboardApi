using ClipboardHelper.FormatProviders;

namespace ClipboardHelper
{
    public class UnicodeTextProvider : StandartUnicodeTextProviderBase
    {
        public UnicodeTextProvider()
            : base(StandartClipboardFormats.UnicodeText)
        {
        }
    }
}