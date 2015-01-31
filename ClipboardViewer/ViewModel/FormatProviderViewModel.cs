using ClipboardHelper.FormatProviders;

namespace ClipboardViewer.ViewModel
{
    class FormatProviderViewModel
    {
        private readonly IClipbordFormatProvider provider;

        public FormatProviderViewModel(IClipbordFormatProvider provider)
        {
            this.provider = provider;
        }

        public virtual string Name
        {
            get
            {
                return provider.FormatId;
            }
        }
    }

    class UnknownFormatProviderViewModel : FormatProviderViewModel
    {
        private readonly UnknownFormatProvider provider;

        public UnknownFormatProviderViewModel(UnknownFormatProvider provider) : base(provider)
        {
            this.provider = provider;
        }

        public override string Name
        {
            get
            {
                return string.Format("{{[Unknown]{0}:{1}}}", provider.Id, provider.FormatId);
            }
        }
    }
}
