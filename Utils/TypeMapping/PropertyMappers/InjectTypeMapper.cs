using System.Reflection;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping.PropertyMappers
{
    public class InjectTypeMapper : InitPropertyMapper
    {
        protected override MethodBase GetInitMethod(IPropertyMappingInfo propInfo)
        {
            return TypeHelpers.TryGetConstructor(propInfo.Type);
        }

        protected override bool InvokeInitMethod(InitMethodInfo initInfo)
        {
            var ctor = initInfo.InitalizerMethod as ConstructorInfo;
            if (ctor == null)
                return false;
            var createdInstance=ctor.Invoke(initInfo.MappingArgs);
            initInfo.PropInfo.SetValue(createdInstance);
            return true;
        }
    }
}
