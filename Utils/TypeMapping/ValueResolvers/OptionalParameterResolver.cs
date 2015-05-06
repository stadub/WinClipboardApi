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

        public OperationResult ResolveSourceValue(PropertyInfo mappingMember, object source)
        {
            return OperationResult.Failed();
        }

        public OperationResult ResolveSourceValue(ParameterInfo paramInfo, object source)
        {
            //optional parametr - defaut value or default(T)
            if (paramInfo.IsOptional)
            {
                var value = paramInfo.HasDefaultValue ? paramInfo.DefaultValue : TypeHelpers.GetDefault(paramInfo.ParameterType);
                return OperationResult.Successful(value);
            }
            return OperationResult.Failed();
        }
      
    }
}