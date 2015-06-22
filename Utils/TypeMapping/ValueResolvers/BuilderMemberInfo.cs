using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers
{
    public enum MemberType
    {
        Parameter, Property
    }

    public class BuilderMemberInfo
    {
        [DebuggerStepThrough]
        public BuilderMemberInfo(ParameterInfo mappingMember)
        {
            Attributes = mappingMember.GetCustomAttributes();
            Type = mappingMember.ParameterType;
            Name = mappingMember.Name;
            MemberType = MemberType.Parameter;
        }

        [DebuggerStepThrough]
        public BuilderMemberInfo(IPropertyMappingInfo mappingMember)
        {
            Attributes = mappingMember.Attributes;
            Type = mappingMember.Type;
            Name = mappingMember.Name;
            MemberType = MemberType.Property;
        }

        public IEnumerable<Attribute> Attributes { get; private set; }
        public Type Type { get; private set; }
        public string Name { get; private set; }
        public MemberType MemberType { get; private set; }
    }

    public class MappingMemberInfo:BuilderMemberInfo
    {
        public object SourceInstance { get; private set; }
        public Type SourceType { get { return SourceInstance.GetType(); } }
        
        [DebuggerStepThrough]
        public MappingMemberInfo(ParameterInfo mappingMember, object source): base(mappingMember)
        {
            SourceInstance = source;
            
        }

        [DebuggerStepThrough]
        public MappingMemberInfo(IPropertyMappingInfo mappingMember, object source)
            : base(mappingMember)
        {
            SourceInstance = source;
        }
    }


    public interface IPropertyMappingInfo
    {
        Type Type { get; }
        string Name { get; }
        IList<Attribute> Attributes { get; }
        object SourceInstance { get; }
        void SetValue(object value);
    }

    public class PropertyMappingInfo : IPropertyMappingInfo
    {

        public Type Type { get; set; }
        public string Name { get; set; }
        public IList<Attribute> Attributes { get; set; }
        public object SourceInstance { get; set; }

        public Action<object> ValueSetter { get; set; }

        public PropertyMappingInfo()
        {
            
        }

        [DebuggerStepThrough]
        public PropertyMappingInfo(PropertyInfo mappingMember, object sourceInstance)
        {
            Type = mappingMember.PropertyType;
            Name=mappingMember.Name;
            ValueSetter = (x) => { mappingMember.SetValue(sourceInstance, x); };
            Attributes = new List<Attribute>(mappingMember.GetCustomAttributes());
            SourceInstance = sourceInstance;
        }

        public void SetValue(object value)
        {
            if (ValueSetter == null)
            {
                Logger.LogError("Value setter is null");
                return;
            }
                
            ValueSetter(value);
        }
    }

    public class MappingItemInfo
    {
        public MappingItemInfo(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }
        public Type Type { get { return Value.GetType(); } }
        public IList<Attribute> Attributes { get; set; }
    }

    public class InitMethodInfo
    {
        public MethodBase InitalizerMethod { get; set; }
        public IPropertyMappingInfo PropInfo { get; set; }
        public object Instance { get; set; }
        public object[] MappingArgs { get; set; }
    }

}