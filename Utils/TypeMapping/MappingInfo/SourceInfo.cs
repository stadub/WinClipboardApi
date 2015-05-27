using System;
using System.Collections.Generic;

namespace Utils.TypeMapping
{
    public class SourceInfo : ISourceInfo
    {
        public SourceInfo(object value)
        {
            Value = value;
            Attributes= new Attribute[0];
        }

        public object Value { get; private set; }
        public IList<Attribute> Attributes { get; set; }

        public static SourceInfo Create(object value)
        {
            return new SourceInfo(value);
        }
    }
}