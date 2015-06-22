using System.Collections.Generic;
using System.Reflection;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping
{
    internal class BuilderUtils
    {
        //String/string structure keys used for identification
        //potentially may work incorrect in the old versions of the dotnet 
        //when Application has different AppDomains
        public static KeyValuePair<string, string> GetKey(PropertyInfo propInfo)
        {
            return new KeyValuePair<string, string>(propInfo.Name, propInfo.PropertyType.FullName);
        }

        public static KeyValuePair<string, string> GetKey(BuilderMemberInfo propInfo)
        {
            return new KeyValuePair<string, string>(propInfo.Name, propInfo.Type.FullName);
        }

        public static KeyValuePair<string, string> GetKey(IPropertyMappingInfo propInfo)
        {
            return new KeyValuePair<string, string>(propInfo.Name, propInfo.Type.FullName);
        }
    }
}