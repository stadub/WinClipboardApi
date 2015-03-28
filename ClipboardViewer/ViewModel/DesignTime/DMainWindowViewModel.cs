using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
using Utils;
using Utils.Wpf.MvvmBase;

namespace ClipboardViewer.ViewModel.DesignTime
{
    public class DMainWindowViewModel : ViewModelBase
    {
        private readonly IClipboard clipboard;
        private readonly Func<IClipbordFormatProvider>[] clipboardFormats;
        private bool autoUpdate;
        private ReadOnlyCollection<FormatProviderViewModel> providers;

        public DMainWindowViewModel()
        {
            ReloadClipboardContent = new RelayCommand(UpdateFormats);
            UpdateFormats();
        }

        public bool AutoUpdate
        {
            get { return autoUpdate; }
            set
            {
                if (autoUpdate == value) return;
                autoUpdate = value;
                base.OnPropertyChanged();
            }
        }

        public ICommand ReloadClipboardContent { get; set; }

        public ReadOnlyCollection<FormatProviderViewModel> Providers
        {
            get { return providers; }
        }

        private static IEnumerable<IClipbordFormatProvider> AllProviders()
        {
            yield return new FileDropProvider();
            yield return new UnicodeFileNameProvider();
            yield return new HtmlFormatProvider();
            yield return new SkypeFormatProvider();
            yield return new UnicodeTextProvider();
        }

        private void UpdateFormats()
        {
            var porviderViewModels= new List<FormatProviderViewModel>();
            foreach (var provider in AllProviders())
            {
                porviderViewModels.Add(new FormatProviderViewModel(provider, FromatProviderType.Default));
            }
            porviderViewModels.Add(new FormatProviderViewModel(new UnknownFormatProvider(1, "someFormat"),FromatProviderType.Unknown));
            porviderViewModels.Add(new FormatProviderViewModel(new UnknownFormatProvider(1, "someFormat"),FromatProviderType.NotImplemented));

            providers = new ReadOnlyCollection<FormatProviderViewModel>(porviderViewModels);
            base.OnPropertyChanged("Providers");
        }

    }

}
