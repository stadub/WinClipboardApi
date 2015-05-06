using System;

namespace Utils.TypeMapping
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class InjectValueAttribute : Attribute
    {
        public object Value { get; set; }
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

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class MapSourcePropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
    
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class UseInitalizerAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
