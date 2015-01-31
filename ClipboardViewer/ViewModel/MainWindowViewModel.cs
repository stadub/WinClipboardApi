using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
using ClipboardViewer.MvvmBase;
using Utils;

namespace ClipboardViewer.ViewModel
{
    public class MainWindowViewModel:ViewModelBase
    {
        private readonly IClipboard clipboard;
        private readonly Func<IClipbordFormatProvider>[] clipboardFormats;
        private bool autoUpdate;

        public MainWindowViewModel(IClipboard clipboard, IEnumerable<Func<IClipbordFormatProvider>> accessibleFromats)
        {
            this.clipboard = clipboard;
            this.clipboardFormats = accessibleFromats as Func<IClipbordFormatProvider>[] ?? accessibleFromats.ToArray();
            clipboardFormats.ForEach(clipboard.RegisterFormatProvider);
            ReloadClipboardContent= new RelayCommand(UpdateFormats);
        }

        public bool AutoUpdate
        {
            get { return autoUpdate; }
            set
            {
                if (autoUpdate == value)return;
                autoUpdate = value;
                base.OnPropertyChanged();
            }
        }

        public ICommand ReloadClipboardContent { get; set; }

        private IEnumerable<IClipbordFormatProvider> GetAvalibleFromats()
        {
            return clipboard.GetAvalibleFromats();
        }

        private void UpdateFormats()
        {
            var formats = GetAvalibleFromats();
        }

    }
}
