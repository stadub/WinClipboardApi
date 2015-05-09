using System.Reflection;

namespace Utils.TypeMapping.PropertyMappers
{
    public class InjectTypeMapper : InitPropertyMapper
    {
        protected override MethodBase GetInitMethod(object instance, PropertyInfo propInfo)
        {
            return TypeHelpers.TryGetConstructor(propInfo.PropertyType);
        }

        protected override bool InvokeInitMethod(InitMethodInfo initInfo)
        {
            var ctor = initInfo.InitalizerMethod as ConstructorInfo;
            if (ctor == null)
                return false;
            var instance=ctor.Invoke(initInfo.MappingArgs);
            initInfo.PropInfo.SetValue(initInfo.Instance,instance);
            return true;
        }
    }
}
