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


        protected override ISourceInfo ResolveSourceValue(MappingMemberInfo memberInfo)
        {
            var attribute = memberInfo.Attributes.FirstOrDefault(x => x is InjectValueAttribute) as InjectValueAttribute;

            if (attribute == null || attribute.Value == null)
            {
                Logger.LogError("InjectValueResolver::ResolveSourceValue", "Mappling value is not set");
                return null;
            }

            if (attribute.Value is string && string.IsNullOrWhiteSpace(attribute.Value.ToString()))
            {
                Logger.LogError("InjectValueResolver::ResolveSourceValue", "Mappling value cannot be empty");
                return null;
            }

            var convertedValue = Convert.ChangeType(attribute.Value, memberInfo.Type);
            return SourceInfo.Create(convertedValue);;
        }

    }
}
