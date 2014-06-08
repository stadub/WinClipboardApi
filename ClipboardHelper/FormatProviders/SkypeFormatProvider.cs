using System;

namespace ClipboardHelper
{
    public class SkypeFormatProvider : DataFormatProvider<string>
    {

        public override string FormatId
        {
            get { return "SkypeMessageFragment"; }
        }

        public override byte[] Serialize(string data)
        {
            throw new NotImplementedException();
        }

        public override string Deserialize(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}