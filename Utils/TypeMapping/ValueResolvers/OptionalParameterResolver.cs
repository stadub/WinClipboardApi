using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers
{
    public class OptionalParameterResolver : ISourceMappingResolver
    {
        public bool IsMemberSuitable(PropertyInfo mappingMember)
        {
            return false;
        }

        public bool IsMemberSuitable(ParameterInfo paramInfo)
        {
            return paramInfo.IsOptional;
        }

        public ISourceInfo ResolveSourceValue(PropertyInfo mappingMember, object source)
        {
            return null;
        }

        public ISourceInfo ResolveSourceValue(ParameterInfo paramInfo, object source)
        {
            //optional parametr - defaut value or default(T)
            if (paramInfo.IsOptional)
            {
                var value = paramInfo.HasDefaultValue ? paramInfo.DefaultValue : TypeHelpers.GetDefault(paramInfo.ParameterType);
                return SourceInfo.Create(value);
            }
            return null;
        }
      
    }
}