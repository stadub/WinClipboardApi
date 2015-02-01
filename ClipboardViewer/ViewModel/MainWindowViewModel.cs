using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
using ClipboardHelper.Watcher;
using Utils;
using Utils.Wpf.MvvmBase;

namespace ClipboardViewer.ViewModel
{

    public class MainWindowViewModel : ViewModelBase,IDisposable
    {
        private readonly IClipboard clipboard;
        private readonly TypeMapper mapper;
        private readonly Func<IClipbordFormatProvider>[] clipboardFormats;
        private bool autoUpdate;
        private ReadOnlyCollection<FormatProviderViewModel> providers;
        private IClipbordWatcher clipbordWatcher;
        private bool showUnknownFormats;

        public MainWindowViewModel(IClipboard clipboard, IEnumerable<Func<IClipbordFormatProvider>> accessibleFromats,TypeMapper mapper)
        {
            this.clipboard = clipboard;

            this.mapper = mapper;
            this.clipboardFormats = accessibleFromats as Func<IClipbordFormatProvider>[] ?? accessibleFromats.ToArray();
            clipboardFormats.ForEach(clipboard.RegisterFormatProvider);

            ReloadClipboardContent = new RelayCommand(UpdateFormats, () => !AutoUpdate);

            Dispatcher.CurrentDispatcher.InvokeAsync(UpdateFormats, DispatcherPriority.Background);
        }

        [InjectValue]
        public IClipbordWatcher ClipbordWatcher
        {
            get { return clipbordWatcher; }
            set
            {
                if (clipbordWatcher != null)
                    ClipbordWatcher.OnClipboardContentChanged -= OnClipboardContentChanged;

                clipbordWatcher = value;
                if (clipbordWatcher != null)
                    ClipbordWatcher.OnClipboardContentChanged += OnClipboardContentChanged;
            }
        }

        private void OnClipboardContentChanged(object sender, EventArgs<uint> eventArgs)
        {
            if (AutoUpdate)
                UpdateFormats();
        }

        public bool AutoUpdate
        {
            get { return autoUpdate; }
            set
            {
                if (autoUpdate == value)return;
                autoUpdate = value;

                if (value)
                    ClipbordWatcher.StartListen();
                OnPropertyChanged();
                ReloadClipboardContent.CanExecute(null);
            }
        }

        public bool ShowUnknownFormats
        {
            get { return showUnknownFormats; }
            set
            {
                if (showUnknownFormats == value) return;
                showUnknownFormats = value;
                OnPropertyChanged();
            }
        }

        public ICommand ReloadClipboardContent { get; set; }

        private IEnumerable<IClipbordFormatProvider> GetAvalibleFromats()
        {
            clipboard.OpenReadOnly();
            var formats = clipboard.GetAvalibleFromats(showUnknownFormats).ToList();
            clipboard.Close();
            return formats;
        }

        public ReadOnlyCollection<FormatProviderViewModel> Providers
        {
            get { return providers; }
        }

        private void UpdateFormats()
        {
            var porviderViewModels = new List<FormatProviderViewModel>();
            foreach (var provider in GetAvalibleFromats())
            {
                FormatProviderViewModel providerVM;
                if (provider is UnknownFormatProvider)
                    providerVM = new UnknownFormatProviderViewModel((UnknownFormatProvider) provider);
                else
                    providerVM= new FormatProviderViewModel(provider);
                porviderViewModels.Add(providerVM);
            }
            providers = new ReadOnlyCollection<FormatProviderViewModel>(porviderViewModels);
            base.OnPropertyChanged("Providers");
        }

        public void Dispose()
        {
            ClipbordWatcher.Stop();
        }
    }
}
