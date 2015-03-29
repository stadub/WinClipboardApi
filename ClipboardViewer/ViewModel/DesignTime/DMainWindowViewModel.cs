using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
            get
            {
                return providers;
            }
        }


        private void UpdateFormats()
        {
            var porviderViewModels = new List<FormatProviderViewModel>();

            porviderViewModels.Add(new FormatProviderViewModel
            {
                Name = "FakeProvoder",
                Data = "[No Data]"
            });

            porviderViewModels.Add(new UnknownFormatViewModel
            {
                Name = "FakeUnknown",
                Bytes = new byte[] {1, 1, 42, 1, 2, 2, 3, 5, 7, 8, 4, 3, 5}
            });


            porviderViewModels.Add(new NotImplementedStandartFormatViewModel
            {
                Name = "FakeNotImplemented",
                FormatName = "CF_FAKE",
                Data = "[No Data]",
                Bytes = new byte[] {1, 1, 42, 1, 2, 2, 3, 5, 7, 8, 4, 3, 5}
            });

            porviderViewModels.Add(new FileDropViewModel
            {
                Name = "FakeFileDrop",
                Text = @"c:\Windows\System32\usr\bin\sh"
            });

            porviderViewModels.Add(new UnicodeTextViewModel
            {
                Name = "FakeText",
                Text = @"Delorem ipsum dolor sit amet, consectetur adipisicing elit"
            });

            porviderViewModels.Add(new FileNameViewModel
            {
                Name = "FakeFileName",
                File = new FileInfo(Environment.GetEnvironmentVariable("USERPROFILE") + "\file")
            });
            porviderViewModels.Add(new FileNameViewModel
            {
                Name = "FakeFileName",
                File = new FileInfo(@"C:\Windows\regedit.exe")
            });
            porviderViewModels.Add(new FileNameViewModel
            {
                Name = "FakeFileName",
                File = new FileInfo(@"c:\Windows\System32\usr\bin\sh")
            });

            porviderViewModels.Add(new HtmlClipboardFormatViewModel
            {
                Name = "FakeFileName",
                Version = "0.1",
                StartHTML = 10,
                EndHTML = 130,
                StartFragment = 20,
                EndFragment = 120,
                StartSelection = 50,
                EndSelection = 120,
                SourceURL = new Uri(@"http://www.example.com/"),
                Html = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN\"">
<HTML dir=ltr><HEAD><TITLE>Lorem ipsum dolor sit amet</TITLE><LINK 
rel=stylesheet type=text/css href=""adipiscing.css"" media=screen></HEAD>
<BODY>
<TABLE>
<TBODY>
<TR>
<TD class=ie><!--StartFragment-->
<P id=Text1>Pellentesque imperdiet consequat lectus sed accumsan. <A id=porta
title=""consectetur"" href=""http://www.w3.org"">Cras et arcu id dui eleifend euismod.</A>.</P>
<P id=Text2> Praesent eu turpis sem.</P><!--EndFragment--></TD></TR></TBODY></TABLE></BODY></HTML> "
            });

            porviderViewModels.Add(new SkypeQuoteFormatViewModel
            {
                Author = "pater_patriae1",
                AuthorName = "Marcus Tullius Cicero",
                Conversation = "#Cicero/$f65b360f0397c5fab",
                Guid = "xbd317d212319792a09a7b384726393691325aa0005a15acfb6bc58a0c29b8c31",
                Timestamp = 1035923063776,
                LegacyQuote = new List<SkypeMessageTextLine>
                {
                    new SkypeMessageTextLine
                    {
                        Text = "[1/10/32 1:10:23 AM] de Finibus Bonorum et Malorum: ",
                        Quote = true
                    },
                    new SkypeMessageTextLine
                    {
                        Text = "Delorem ipsum dolor sit amet, consectetur adipisicing elit," +
                               " sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
                    },
                    new SkypeMessageTextLine {Text = "<<< ", Quote = true},
                },
            });

            providers = new ReadOnlyCollection<FormatProviderViewModel>(porviderViewModels);

            base.OnPropertyChanged("Providers");
        }

    }
}
