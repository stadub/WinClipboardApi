using System;

namespace Utils.TypeMapping
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class InjectValueAttribute : Attribute
    {
        public object Value { get; set; }
        public InjectValueAttribute()
        {
        }
        public InjectValueAttribute(string value)
        {
            Value = value;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class ShoudlInjectAttribute : Attribute
    {
        public ShoudlInjectAttribute()
        {
        }
    }
}
