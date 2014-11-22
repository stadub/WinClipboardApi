using System;

namespace ClipboardHelper.FormatProviders
{
    public class UnknownFormatProvider:  IClipbordFormatProvider
    {
        internal UnknownFormatProvider(uint id)
        {
            this.Id = id;
        }

        public string FormatId { get { return string.Empty; } }
        public uint Id { get; private set; }

        public byte[] Serialize(object data)
        {
            throw new NotSupportedException();
        }

        public object Deserialize(byte[] data)
        {
            throw new NotSupportedException();
        }
    }
}
