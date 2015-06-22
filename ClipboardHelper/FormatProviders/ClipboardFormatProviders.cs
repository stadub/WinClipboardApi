using System;
using System.IO;
using System.Text;

namespace ClipboardHelper.FormatProviders
{
    public interface IClipbordFormatProvider
    {
        string FormatId { get; }
        byte[] Serialize();

        void Deserialize(byte[] data);

        object Data { get; }
    }
    
    //extend provider

    public abstract class DataFormatProvider : IClipbordFormatProvider
    {

        public abstract string FormatId { get; }

        public abstract byte[] Serialize();


        public void Deserialize(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            DeserializeData(data);
        }

        public abstract object Data { get; }

        protected abstract void DeserializeData(byte[] data);


        protected bool Equals(DataFormatProvider other)
        {
            if (ReferenceEquals(other, this))
                return true;
            if (other == null) return false;
            return other.FormatId == this.FormatId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DataFormatProvider);
        }

        public override string ToString()
        {
            return string.Format("{{{0} FormatId:{1}}}", this.GetType().Name, FormatId);
        }
    }


    
    public class UnicodeStringSerializer
    {
        public byte[] Serialize(string data)
        {
            data += '\0';
            return Encoding.Unicode.GetBytes(data);
        }


        public string Deserialize(byte[] data)
        {
            Array.Resize(ref data, data.Length - 2);
            return Encoding.Unicode.GetString(data);
        }
    }

    public class UTF8StringSerializer
    {
        public byte[] Serialize(string data)
        {
            data += '\0';
            return Encoding.Unicode.GetBytes(data);
        }


        public string Deserialize(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }

    public abstract class StandartClipboardFormatBase : DataFormatProvider
    {
        protected StandardFormatIdWraper formatIdWraper;

        public StandartClipboardFormatBase(StandartClipboardFormats standartClipboardFormat)
        {
            this.formatIdWraper = new StandardFormatIdWraper(standartClipboardFormat);
        }
    }

    public abstract class StandartUnicodeTextProviderBase : StandartClipboardFormatBase
    {

        private UnicodeStringSerializer provider;
        public StandartUnicodeTextProviderBase(StandartClipboardFormats standartClipboardFormat)
            : base(standartClipboardFormat)
        {
            provider = new UnicodeStringSerializer();
            Text = string.Empty;
        }
        public override string FormatId
        {
            get { return formatIdWraper.FormatName; }
        }

        public override object Data { get { return Text; } }
        public string Text { get; set; }

        public override byte[] Serialize()
        {
            return provider.Serialize(Text);
        }

        protected override void DeserializeData(byte[] data)
        {
            Text = provider.Deserialize(data);
        }
    }


    public class FileDropProvider :StandartUnicodeTextProviderBase
    {
        public FileDropProvider(): base(StandartClipboardFormats.HDrop){}
    }

    public class UnicodeFileNameProvider :  DataFormatProvider
    {
        private UnicodeStringSerializer provider;
        public UnicodeFileNameProvider()
        {
            provider = new UnicodeStringSerializer();
        }
        public override string FormatId
        {
            get { return "FileNameW"; }
        }

        public FileInfo File { get; set; }

        public override byte[] Serialize()
        {
            return provider.Serialize(File.FullName);
        }

        public override object Data
        {
            get { return File; }
        }

        protected override void DeserializeData(byte[] data)
        {
            var filePath = provider.Deserialize(data);
            File= new FileInfo(filePath);
        }
    }
}
