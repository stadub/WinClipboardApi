using System.Linq;
using System.Reflection;
using Utils.TypeMapping.TypeBuilders;

namespace Utils.TypeMapping.ValueResolvers
{
    class SourceAttributeResolver : SourcePropertyResolver
    {

        protected override OperationResult ResolveSourceValue(MappingMemberInfo memberInfo)
        {
            var sourceValue = memberInfo.SourceInstance;

            var srcPropMap =
                memberInfo.Attributes.FirstOrDefault(x => x is MapSourcePropertyAttribute) as MapSourcePropertyAttribute;
            object propValue = null;
            MappingResult result = MappingResult.NotResolved;

            if (srcPropMap.Name != null && srcPropMap.Path != null)
                throw new PropertyMappingException(memberInfo.Type.FullName, memberInfo.Name,
                    "Either Name or Path in the MapSourcePropertyAttribute should be set.");


            if (!string.IsNullOrWhiteSpace(srcPropMap.Name))
            {
                return GetPropertyByName(memberInfo, srcPropMap, sourceValue);
            }
            if (!string.IsNullOrWhiteSpace(srcPropMap.Path))
            {
                return GetProperyByPath(srcPropMap, sourceValue);
            }
            return OperationResult.Failed();
        }

        private OperationResult GetPropertyByName(MappingMemberInfo memberInfo, MapSourcePropertyAttribute srcPropMap,
            object sourceValue)
        {
            var sourceType = memberInfo.SourceType;
            var srcProp = TryFindAppropriateProperty(srcPropMap.Name, sourceType);

            var value = srcProp.GetValue(sourceValue);

            return OperationResult.Successful(value);
        }

        private OperationResult GetProperyByPath(MapSourcePropertyAttribute srcPropMap, object sourceValue)
        {
            object propValue;
            var path = srcPropMap.Path.Split('.');

            PropertyInfo prop;
            propValue = sourceValue;

            for (int i = 0; i < path.Length; i++)
            {
                prop = TryFindAppropriateProperty(path[i], propValue.GetType());
                if (prop == null)
                {
                    propValue = null;
                    break;
                }
                propValue = prop.GetValue(propValue);
                if (propValue == null) return OperationResult.Failed();
            }
            return OperationResult.Successful(propValue);
        }

        protected override bool IsMemberSuitable(BuilderMemberInfo memberInfo)
        {
            return memberInfo.Attributes.Any(attribute => attribute is MapSourcePropertyAttribute);
        }

    
    }
}
