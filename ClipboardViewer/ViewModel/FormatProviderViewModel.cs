using System;
using System.IO;
using ClipboardHelper.FormatProviders;
using Utils;
using System.Collections.Generic;

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
        [SourceProperty(Name = "FormatId")]
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
        [SourceProperty(Path = "HtmlData.Version")]
        public string Version { get; set; }

        [SourceProperty(Path = "HtmlData.StartHTML")]
        public long StartHTML { get; set; }

        [SourceProperty(Path = "HtmlData.EndHTML")]
        public long EndHTML { get; set; }

        [SourceProperty(Path = "HtmlData.StartFragment")]
        public long StartFragment { get; set; }

        [SourceProperty(Path = "HtmlData.EndFragment")]
        public long EndFragment { get; set; }

        [SourceProperty(Path = "HtmlData.StartSelection")]
        public long StartSelection { get; set; }

        [SourceProperty(Path = "HtmlData.EndSelection")]
        public long EndSelection { get; set; }

        [SourceProperty(Path = "HtmlData.SourceURL")]
        public Uri SourceURL { get; set; }

        [SourceProperty(Path = "HtmlData.Html")]
        public string Html { get; set; }

    }

    public class SkypeMessageTextLine
    {
        public string Text { get; set; }
        public bool Quote { get; set; }
    }


    public class SkypeQuoteFormatViewModel : FormatProviderViewModel
    {
        [SourceProperty(Path = "Quote.Author")]
        public string Author { get; set; }

        [SourceProperty(Path = "Quote.AuthorName")]
        public string AuthorName { get; set; }

        [SourceProperty(Path = "Quote.Conversation")]
        public string Conversation { get; set; }

        [SourceProperty(Path = "Quote.Guid")]
        public string Guid { get; set; }

        [SourceProperty(Path = "Quote.Timestamp")]
        public long Timestamp { get; set; }

        [SourceProperty(Path = "Quote.LegacyQuote")]
        public List<SkypeMessageTextLine> LegacyQuote { get; set; }


    }

}
