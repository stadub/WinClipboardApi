using System;
using System.Linq;

namespace Utils.TypeMapping.ValueResolvers
{
    class InjectValueResolver : SourceMappingResolverBase
    {

        protected override bool IsMemberSuitable(BuilderMemberInfo memberInfo)
        {
            return memberInfo.Attributes.Any(x => x is InjectValueAttribute);
        }


        protected override OperationResult ResolveSourceValue(MappingMemberInfo memberInfo)
        {
            var attribute = memberInfo.Attributes.FirstOrDefault(x => x is InjectValueAttribute) as InjectValueAttribute;

            if (attribute==null || attribute.Value == null)
                return OperationResult.Failed(new ArgumentException("Mappling value is not set", "Value"));

            if (attribute.Value is string && string.IsNullOrWhiteSpace(attribute.Value.ToString()))
                return OperationResult.Failed(new ArgumentException("Mappling value cannot be empty", "Value"));


            var convertedType = Convert.ChangeType(attribute.Value, memberInfo.Type);
            return OperationResult.Successful(convertedType);
        }

    }
}
