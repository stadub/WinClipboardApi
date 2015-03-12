using ClipboardHelper.FormatProviders;

namespace ClipboardViewer.ViewModel
{
    public class FormatProviderViewModelFactory
    {
        public FormatProviderViewModel CreateProviderViewModel(IClipbordFormatProvider provider)
        {
            switch (provider.GetType().Name)
            {
                case "UnknownFormatProvider":
                    return new FormatProviderViewModel(provider, FromatProviderType.Unknown);
                case "NotImplementedStandartFormat":
                    return new FormatProviderViewModel(provider, FromatProviderType.NotImplemented);
                default:
                    return new FormatProviderViewModel(provider, FromatProviderType.Default);
            }
        }
    }

    public enum FromatProviderType
    {
        Default,
        Unknown, 
        NotImplemented

    }
    public class FormatProviderViewModel
    {
        private readonly IClipbordFormatProvider provider;

        public FormatProviderViewModel(IClipbordFormatProvider provider, FromatProviderType providerType)
        {
            this.provider = provider;
            ProviderType = providerType;
        }

        public virtual string Name
        {
            get
            {
                return provider.FormatId;
            }
        }
        public FromatProviderType ProviderType { get; private set; }
    }
}
