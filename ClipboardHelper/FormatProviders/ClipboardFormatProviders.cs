using System;
using System.IO;
using System.Text;

namespace ClipboardHelper
{
    public interface IClipbordFormatProvider<T> : IClipbordFormatProvider
    {
        string FormatId { get; }
        byte[] Serialize(T data);

        T Deserialize(byte[] data);
    }
    
    public interface IClipbordFormatProvider
    {
        string FormatId { get; }
        byte[] Serialize(object data);

        object Deserialize(byte[] data);
    }

    //extend provider

    public abstract class DataFormatProvider<T> : IClipbordFormatProvider<T>, IClipbordFormatProvider
    {

        public abstract string FormatId { get; }

        public abstract byte[] Serialize(T data);

        byte[] IClipbordFormatProvider.Serialize(object data)
        {
            if(data==null)
                throw new ArgumentNullException("data");
            if ( !(data is T))
                throw new ArgumentException("data", string.Format("Cannot cast value of {0} to {1} type",data.GetType(),typeof(T)));
            return Serialize((T) data);
        }

        object IClipbordFormatProvider.Deserialize(byte[] data)
        {
            return Deserialize(data);
        }

        public abstract T Deserialize(byte[] data);


        protected bool Equals(DataFormatProvider<T> other)
        {
            if (ReferenceEquals(other, this))
                return true;
            if (other == null) return false;
            return other.FormatId == this.FormatId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DataFormatProvider<T>);
        }

        //public override int GetHashCode()
        //{
        //    FormatId
        //}

        public override string ToString()
        {
            return string.Format("{{{0} FormatId:{1}}}", this.GetType().Name, FormatId);
        }
    }

    public class FormatedAttribute : Attribute
    {
        public FormatedAttribute(string format)
        {
            this.Format = format;
        }

        public string Format { get; set; }
    }
    public class NonSerializableAttribute : Attribute
    {
    }
    public class FormatedNumericAttribute : FormatedAttribute
    {
        public FormatedNumericAttribute() : base("D")
        {
        }
        public FormatedNumericAttribute(string format) : base(format)
        {
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
    public abstract class StandartUnicodeTextProviderBase: DataFormatProvider<string>
    {
        private readonly StandardFormatIdWraper formatIdWraper;
        private UnicodeStringSerializer provider;
        public StandartUnicodeTextProviderBase(StandartClipboardFormats standartClipboardFormat)
        {
            this.formatIdWraper = new StandardFormatIdWraper(standartClipboardFormat);
            provider = new UnicodeStringSerializer();
        }
        public override string FormatId
        {
            get { return formatIdWraper.FormatName; }
        }

        public override byte[] Serialize(string data)
        {
            return provider.Serialize(data);
        }
        
        public override string Deserialize(byte[] data)
        {
            return provider.Deserialize(data);
        }
    }


    public class FileDropProvider :StandartUnicodeTextProviderBase
    {
        public FileDropProvider(): base(StandartClipboardFormats.HDrop){}
    }

    public class UnicodeFileNameProvider :  DataFormatProvider<FileInfo>
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

        public override byte[] Serialize(FileInfo file)
        {
            return provider.Serialize(file.FullName);
        }

        public override FileInfo Deserialize(byte[] data)
        {
            var filePath= provider.Deserialize(data);
            return new FileInfo(filePath);
        }
    }
}
