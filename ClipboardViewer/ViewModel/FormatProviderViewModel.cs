using System;
using System.IO;
using ClipboardHelper.FormatProviders;
using Utils;
using System.Collections.Generic;
using System.Linq;

namespace ClipboardViewer.ViewModel
{
    public enum FromatProviderType
    {
        Default,
        Unknown, 
        NotImplemented
    }

    public class FormatProviderViewModel
    {
        [MapSourceProperty(Name = "FormatId")]
        public string Name { get; set; }

        public object Data { get; set; }

        public virtual FromatProviderType ProviderType
        {
            get { return FromatProviderType.Default;}
        }
    }

    public class UnknownFormatViewModel : FormatProviderViewModel
    {
        public uint Id { get; set; }
        
        public byte[] Bytes { get; set; }

        public override FromatProviderType ProviderType
        {
            get { return FromatProviderType.Unknown; }
        }
    }

    public class NotImplementedStandartFormatViewModel: FormatProviderViewModel
    {

        public byte[] Bytes { get; set; }

        public override FromatProviderType ProviderType
        {
            get { return FromatProviderType.NotImplemented;}
        }
        public string FormatName { get; set; }
    }

    public class FileDropViewModel : FormatProviderViewModel
    {

        public string Text { get; set; }
    }

    public class UnicodeTextViewModel : FormatProviderViewModel
    {

        public string Text { get; set; }
    }

    public class FileNameViewModel : FormatProviderViewModel
    {
        public FileInfo File { get; set; }

        public string FileName { get { return File.Name; } }
        public string Extension { get { return File.Extension; } }

        public string DirectoryName { get { return File.DirectoryName; } }

        public bool Exists { get { return File.Exists; } }

        public string Attributes { get { return File.Attributes.ToString(); } }

        public bool IsReadOnly { get { return File.IsReadOnly; } }

        public DateTime CreationTime { get { return File.CreationTime; } }
        public DateTime LastAccessTime { get { return File.LastAccessTime; } }
        public DateTime LastWriteTime { get { return File.LastWriteTime; } }

        public long Length { get { return File.Length; } }

    }

    public class HtmlClipboardFormatViewModel : FormatProviderViewModel
    {
        [MapSourceProperty(Path = "HtmlData.Version")]
        public string Version { get; set; }

        [MapSourceProperty(Path = "HtmlData.StartHTML")]
        public long StartHTML { get; set; }

        [MapSourceProperty(Path = "HtmlData.EndHTML")]
        public long EndHTML { get; set; }

        [MapSourceProperty(Path = "HtmlData.StartFragment")]
        public long StartFragment { get; set; }

        [MapSourceProperty(Path = "HtmlData.EndFragment")]
        public long EndFragment { get; set; }

        [MapSourceProperty(Path = "HtmlData.StartSelection")]
        public long StartSelection { get; set; }

        [MapSourceProperty(Path = "HtmlData.EndSelection")]
        public long EndSelection { get; set; }

        [MapSourceProperty(Path = "HtmlData.SourceURL")]
        public Uri SourceURL { get; set; }

        [MapSourceProperty(Path = "HtmlData.Html")]
        public string Html { get; set; }

    }

    public class SkypeMessageTextLineViewModel
    {
        public string Text { get; set; }
        public bool Quote { get; set; }
    }


    public class SkypeQuoteFormatViewModel : FormatProviderViewModel
    {
        public SkypeQuoteFormatViewModel()
        {
            LegacyQuote= new List<SkypeMessageTextLineViewModel>();
        }

        [MapSourceProperty(Path = "Quote.Author")]
        public string Author { get; set; }

        [MapSourceProperty(Path = "Quote.AuthorName")]
        public string AuthorName { get; set; }

        [MapSourceProperty(Path = "Quote.Conversation")]
        public string Conversation { get; set; }

        [MapSourceProperty(Path = "Quote.Guid")]
        public string Guid { get; set; }

        [MapSourceProperty(Path = "Quote.Timestamp")]
        public long Timestamp { get; set; }

        [MapSourceProperty(Path = "Quote.LegacyQuote", UseInitalizer = "InitQuoteMessageLines")]
        public IList<SkypeMessageTextLineViewModel> LegacyQuote { get; set; }


        public void InitQuoteMessageLines(IList<SkypeMessageTextLine> legacyQuote)
        {
            var propMapper = new TypeMapper<SkypeMessageTextLine, SkypeMessageTextLineViewModel>();
            LegacyQuote = legacyQuote.Select(line => propMapper.Map(line)).ToList();
        }

    }

}
