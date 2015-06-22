using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows.Input;
using System.Windows.Threading;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
using ClipboardHelper.Watcher;
using Utils;
using Utils.TypeMapping;
using Utils.Wpf.MvvmBase;

namespace ClipboardViewer.ViewModel
{

    public class MainWindowViewModel : ViewModelBase,IDisposable
    {
        public ICommand Loaded { get; set; }

        private readonly IClipboard clipboard;

        private readonly Func<IClipbordFormatProvider>[] clipboardFormats;
        private bool autoUpdate;
        private ReadOnlyCollection<FormatProviderViewModel> providers;
        private IClipbordWatcher clipbordWatcher;
        private bool loadNotImplementedFormats;
        private FormatProviderViewModel provider;

        static int i;

        public MainWindowViewModel(IClipboard clipboard, IEnumerable<Func<IClipbordFormatProvider>> accessibleFromats)
        {
            i++;
            this.clipboard = clipboard;

            this.clipboardFormats = accessibleFromats.ToArray();
            clipboardFormats.ForEach(clipboard.RegisterFormatProvider);

            ReloadClipboardContent = new RelayCommand(UpdateFormats, () => !AutoUpdate);

            Dispatcher.CurrentDispatcher.InvokeAsync(UpdateFormats, DispatcherPriority.Background);
        }

        [ShoudlInject]
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

        [InjectInstance("FormatProviders")]
        public ITypeMapperRegistry ClipboardForamtMapper { get; set; }


        [ShoudlInject]
        public IClipbordMessageProvider MessageProvider { get; set; }

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
                ReloadClipboardContent.RefreshCanExecute();
            }
        }

        public bool LoadNotImplementedFormats
        {
            get { return loadNotImplementedFormats; }
            set
            {
                if (loadNotImplementedFormats == value) return;
                loadNotImplementedFormats = value;

                UpdateFormats();

                OnPropertyChanged();
            }
        }

        public IRelayCommand ReloadClipboardContent { get; set; }

        
        private IEnumerable<IClipbordFormatProvider> GetAvalibleFromats()
        {
            lock (clipboard)
            {
                using (var clipboardReader = clipboard.CreateReader())
                {
                    var formats = clipboardReader.GetAvalibleFromats(loadNotImplementedFormats).ToArray();
                    clipboardReader.Close();
                    return formats;
                }
            }
        }

        public ReadOnlyCollection<FormatProviderViewModel> Providers
        {
            get { return providers; }
        }


        public FormatProviderViewModel Provider
        {
            get { return provider; }
            set
            {
                if (provider == value) return;
                provider = value;
                OnPropertyChanged();
            }
        }


        private void UpdateFormats()
        {
            var porviderViewModels = new List<FormatProviderViewModel>();
            foreach (IClipbordFormatProvider provider in GetAvalibleFromats())
            {
                lock (clipboard)
                {
                    using (var clipboardReader = clipboard.CreateReader())
                    {
                        clipboardReader.GetData(provider);
                        clipboardReader.Close();
                    }
                }
                var providerVms= ClipboardForamtMapper.ResolveDescendants<FormatProviderViewModel>(provider);

                porviderViewModels.AddRange(providerVms);
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
