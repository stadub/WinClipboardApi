using System;
using System.Reflection.Emit;
using System.Text;

namespace ClipboardHelper
{
    public interface IClipbordFormatProvider<T>
    {
        string FormatId { get; }
        byte[] Serialize(T data);

        T Deserialize(byte[] data);
    }

    //extend provider

    public abstract class DataFormatProvider<T> : IClipbordFormatProvider<T>
    {

        public abstract string FormatId { get; }

        public abstract byte[] Serialize(T data);

        public abstract T Deserialize(byte[] data);

    }

    public class UnicodeTextProvider : DataFormatProvider<string>
    {
        private readonly StandardFormatIdWraper formatIdWraper;
        public UnicodeTextProvider()
        {
            formatIdWraper= new StandardFormatIdWraper(StandartClipboardFormats.UnicodeText);
        }
        public override string FormatId
        {
            get { return formatIdWraper.FormatName; }
        }

        public override byte[] Serialize(string data)
        {
            data += '\0';
            return Encoding.Unicode.GetBytes(data);
        }


        public override string Deserialize(byte[] data)
        {
            Array.Resize(ref data, data.Length - 2);
            return Encoding.Unicode.GetString(data);
        }
    }
    
    //public class AcsiiTextProvider : DataFormatProvider<string>
    //{
    //    public AcsiiTextProvider(Clipboard clipboard) : base(clipboard)
    //    {
    //    }

    //    protected override string FormatId
    //    {
    //        get { return "Locale"; }
    //    }

    //    public override void Serialize(string data)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override bool CanSerialize(string data)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override string Deserialize()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class SkypeFormatProvider : UnicodeTextProvider
    {
        public SkypeFormatProvider(Clipboard clipboard) 
        {
        }

        public override string FormatId
        {
            get { return "SkypeMessageFragment"; }
        }

        
    }
}
