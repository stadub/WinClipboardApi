using System;
using System.Reflection;

namespace Utils.TypeMapping.PropertyMappers
{
    public class InitPropertyMapper : IPropertyMapper
    {
        public bool MapPropery(ITypeMapper mapper, PropertyInfo propInfo, object sourceValue, object instance)
        {
            Exception exception;
            MethodBase initalizerMethod = null;
            try
            {
                initalizerMethod = GetInitMethod(instance, propInfo);
            }
            catch (AmbiguousMatchException ex) { exception = ex; }
            catch (ArgumentNullException ex) { exception = ex; }

            if (initalizerMethod == null)
                return false;
            try
            {
                 var parameters=initalizerMethod.GetParameters();
                if (parameters.Length != 1)
                    return false;
                    //throw new ArgumentException("Only initalizers with single argument are supported.");

                var paramType = parameters[0].ParameterType;
                var mappingResult = mapper.Map(sourceValue, paramType);
                if (mappingResult.Success)
                {
                    var invokationInfo = new InitMethodInfo{
                        InitalizerMethod=initalizerMethod,
                        PropInfo=propInfo,
                        Instance = instance,
                        MappingArgs = new[] { mappingResult.Value }
                    };

                    return InvokeInitMethod(invokationInfo);
                }
                return false;
            }
            catch (TargetException ex) { exception = ex; }
            catch (ArgumentException ex) { exception = ex; }
            catch (TargetInvocationException ex) { exception = ex; }
            catch (TargetParameterCountException ex) { exception = ex; }
            catch (MethodAccessException ex) { exception = ex; }
            catch (InvalidOperationException ex) { exception = ex; }
            catch (NotSupportedException ex) { exception = ex; }
            throw exception;
        }

        protected virtual bool InvokeInitMethod(InitMethodInfo initInfo)
        {
            initInfo.InitalizerMethod.Invoke(initInfo.Instance, initInfo.MappingArgs);
            return true;
        }

        protected virtual MethodBase GetInitMethod(object instance, PropertyInfo propInfo)
        {
            var destType = instance.GetType();
            var initalizer = propInfo.GetCustomAttribute<UseInitalizerAttribute>();

            if (initalizer == null)
                return null;
            return destType.GetMethod(initalizer.Name);
        }

        public class InitMethodInfo
        {
            public MethodBase InitalizerMethod { get; set; }
            public PropertyInfo PropInfo { get; set; }
            public object Instance { get; set; }
            public object[] MappingArgs { get; set; }
        }
    }
}
