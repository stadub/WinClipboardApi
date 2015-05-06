using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers
{
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
        public BuilderMemberInfo(PropertyInfo mappingMember)
        {
            Attributes = mappingMember.GetCustomAttributes();
            Type = mappingMember.PropertyType;
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
        public MappingMemberInfo(PropertyInfo mappingMember, object source):base(mappingMember)
        {
            SourceInstance = source;
        }
    }

    public enum MemberType
    {
        Parameter, Property
    }
}