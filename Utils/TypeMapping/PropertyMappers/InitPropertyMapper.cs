using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping.PropertyMappers
{
    public class InitPropertyMapper : IPropertyMapper
    {
        public bool MapPropery(ITypeMapper mapper, IPropertyMappingInfo propInfo, object sourceValue, IList<Attribute> metadata = null)
        {
            if (!mapper.CanMap(sourceValue, propInfo.Type))
                return false;
            Exception exception;
            MethodBase initalizerMethod = null;
            try
            {
                initalizerMethod = GetInitMethod(propInfo);
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
                if (!mapper.CanMap(sourceValue, paramType))
                    return false;

                IOperationResult mappingResult;
                if (mapper is ITypeInfoMapper)
                    mappingResult = ((ITypeInfoMapper)mapper).Map(new SourceInfo(sourceValue){Attributes = metadata}, paramType);
                else
                    mappingResult = mapper.Map(sourceValue, paramType);
                if (mappingResult.Success)
                {
                    var invokationInfo = new InitMethodInfo{
                        InitalizerMethod=initalizerMethod,
                        PropInfo=propInfo,
                        Instance = propInfo.SourceInstance,
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

        protected virtual MethodBase GetInitMethod(IPropertyMappingInfo propInfo)
        {
            var destType = propInfo.SourceInstance.GetType();
            var initalizer = propInfo.Attributes.FirstOrDefault(x => x is UseInitalizerAttribute) as UseInitalizerAttribute;

            if (initalizer == null)
                return null;
            return destType.GetMethod(initalizer.Name);
        }
    }
}
