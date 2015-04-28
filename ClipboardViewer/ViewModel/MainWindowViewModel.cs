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
        private readonly IClipboard clipboard;

        private readonly Func<IClipbordFormatProvider>[] clipboardFormats;
        private bool autoUpdate;
        private ReadOnlyCollection<FormatProviderViewModel> providers;
        private IClipbordWatcher clipbordWatcher;
        private bool loadNotImplementedFormats;
        private FormatProviderViewModel provider;

        public MainWindowViewModel(IClipboard clipboard, IEnumerable<Func<IClipbordFormatProvider>> accessibleFromats)
        {
            this.clipboard = clipboard;

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

        [InjectValue]
        ITypeMapperRegistry ClipboardForamtMapper { get; set; }

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

                if (ReloadClipboardContent.CanExecute())
                    ReloadClipboardContent.Execute();

                OnPropertyChanged();
            }
        }

        public IRelayCommand ReloadClipboardContent { get; set; }

        
        private IEnumerable<IClipbordFormatProvider> GetAvalibleFromats()
        {
            lock (clipboard)
            {
                clipboard.OpenReadOnly();
                var formats = clipboard.GetAvalibleFromats(loadNotImplementedFormats).ToList();
                clipboard.Close();
                return formats;
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
