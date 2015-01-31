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
        private readonly TypeMapper mapper;
        private readonly IClipboard clipboard;
        private readonly Func<IClipbordFormatProvider>[] clipboardFormats;
        private bool autoUpdate;

        public DMainWindowViewModel(TypeMapper mapper)
        {
            this.mapper = mapper;
            ReloadClipboardContent = new RelayCommand(UpdateFormats);
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

        //public ReadOnlyObservableCollection<KeyValuePair<string,string>> 

        private void UpdateFormats()
        {
            
        }

    }

}
