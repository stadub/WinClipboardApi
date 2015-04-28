using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers
{
    class ValueInjector : SourceMappingResolverBase
    {

        protected override bool IsMemberSuitable(IEnumerable<Attribute> memberAttributes)
        {
            return memberAttributes.Any(x=>x is InjectValueAttribute);
        }

        protected override OperationResult<object> ResolveSourceValue(string memberName, Type memberType, IEnumerable<Attribute> memberAttributes)
        {
            var attribute = memberAttributes.FirstOrDefault(x => x is InjectValueAttribute) as InjectValueAttribute;

            if (attribute.Value == null)
                return OperationResult.Failed(new ArgumentException("Mappling value is not set", "Value"));

            if (attribute.Value is string && string.IsNullOrWhiteSpace(attribute.Value.ToString()))
                return OperationResult.Failed(new ArgumentException("Mappling value cannot be empty", "Value"));


            var convertedType = Convert.ChangeType(attribute.Value,memberType);
            return OperationResult.Successfull(convertedType);
        }
    }
}
