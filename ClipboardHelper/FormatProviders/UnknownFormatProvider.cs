namespace ClipboardHelper.FormatProviders
{
    public class UnknownFormatProvider:  IClipbordFormatProvider
    {
        public UnknownFormatProvider(uint id,string formatId)
        {
            FormatId = formatId;
            this.Id = id;
        }

        public string FormatId { get; private set; }
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
