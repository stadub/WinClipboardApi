using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers
{
    class SourcePropertyResolver : SourceMappingResolverBase
    {
        protected override bool IsMemberSuitable(BuilderMemberInfo memberInfo)
        {
            return true;
        }

        protected override OperationResult ResolveSourceValue(MappingMemberInfo memberInfo)
        {
            var sourceValue = memberInfo.SourceInstance;
            var sourceType = memberInfo.SourceType;

            var propInfo = TryFindAppropriateProperty(memberInfo.Name, sourceType);
            if (propInfo == null)
                return OperationResult.Failed();
            var value = propInfo.GetValue(sourceValue);
            return OperationResult.Successful(value);
        }

        private static bool NameIsSame(PropertyInfo property, string name)
        {
            return NameIsSame(property.Name, name);
        }

        private static bool NameIsSame(string propName, string name)
        {
            return propName.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        protected static List<PropertyInfo> EnumerateSourceProperties(Type @type)
        {
            return @type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead)
                .ToList();
        }

        protected virtual PropertyInfo TryFindAppropriateProperty(string name, Type @type)
        {

            foreach (PropertyInfo sourceProperty in EnumerateSourceProperties(@type))
            {
                if (NameIsSame(sourceProperty, name))
                    return sourceProperty;
            }
            return null;
        }   
    }
}