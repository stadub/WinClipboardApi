using Utils.Wpf.MvvmBase;

namespace ClipboardViewer.ViewModel.ClipboardContent
{
    class TextClipboardContentViewModel:ViewModelBase
    {
        public TextClipboardContentViewModel(string text)
        {
            Text = text;
        }

        private string text;

        public string Text
        {
            get { return text; }
            private set
            {
                text = value;
                OnPropertyChanged();
            }
        }
    }
}
