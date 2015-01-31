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

        public object Data { get { return Bytes; } }
        public byte[] Bytes { get; set; }

        public byte[] Serialize()
        {
            return Bytes;
        }

        public void Deserialize(byte[] data)
        {
            Bytes = data;
        }

    }
}
