using System;
using System.Reflection;

namespace Utils.TypeMapping.PropertyMappers
{
    public class InitPropertyMapper : IPropertyMapper
    {
        public bool MapPropery(ITypeMapper mapper, PropertyInfo propInfo, object sourceValue, object instance)
        {

            var initalizer=propInfo.GetCustomAttribute<UseInitalizerAttribute>();

            if (initalizer == null)
                return false;

            
            //return false;
            var destType = instance.GetType();
            Exception exception;
            try
            {
                var initalizerMethod=destType.GetMethod(initalizer.Name);

                var parameters=initalizerMethod.GetParameters();
                if (parameters.Length != 1)
                    throw new ArgumentException("Only initalizers with single argument are supported.");

                var paramType = parameters.GetType();
                var mappingResult = mapper.Map(sourceValue, paramType);
                if (mappingResult.Success)
                {
                    initalizerMethod.Invoke(instance, new[] { mappingResult.Value });
                    return true;
                }
                return false;
            }
            catch (AmbiguousMatchException ex) { exception = ex; }
            catch (TargetException ex) { exception = ex; }
            catch (ArgumentException ex) { exception = ex; }
            catch (TargetInvocationException ex) { exception = ex; }
            catch (TargetParameterCountException ex) { exception = ex; }
            catch (MethodAccessException ex) { exception = ex; }
            catch (InvalidOperationException ex) { exception = ex; }
            catch (NotSupportedException ex) { exception = ex; }
            throw exception;
        }
    }
}
