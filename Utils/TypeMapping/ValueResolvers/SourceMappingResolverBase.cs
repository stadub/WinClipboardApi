using System;
using System.Collections.Generic;
using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers
{
    public abstract class SourceMappingResolverBase : ISourceMappingResolver
    {
        public bool IsMemberSuitable(PropertyInfo mappingMember)
        {
            var attributes = mappingMember.GetCustomAttributes();
            return IsMemberSuitable(attributes);
        }

        public bool IsMemberSuitable(ParameterInfo mappingMember)
        {
            var attributes = mappingMember.GetCustomAttributes();
            return IsMemberSuitable(attributes);
        }

        public OperationResult<object> ResolveSourceValue(PropertyInfo mappingMember)
        {
            return ResolveSourceValue(mappingMember.Name, mappingMember.PropertyType,
                mappingMember.GetCustomAttributes());
        }

        public OperationResult<object> ResolveSourceValue(ParameterInfo mappingMember)
        {
            return ResolveSourceValue(mappingMember.Name, mappingMember.ParameterType,mappingMember.GetCustomAttributes());
        }

        protected abstract bool IsMemberSuitable(IEnumerable<Attribute> memberAttributes);

        protected abstract OperationResult<object> ResolveSourceValue(string memberName,Type memberType,IEnumerable<Attribute> memberAttributes);
    }
}