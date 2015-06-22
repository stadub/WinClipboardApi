using System;
using System.ComponentModel;

namespace Utils.TypeMapping.TypeMappers
{
    public class ConvertTypeMapper<TSource, TDest> :ConvertTypeMapper, ITypeMapper<TSource, TDest>
    {
        public bool CanMap(TSource source)
        {
            return base.CanMap(source, typeof (TDest));
        }

        public IOperationResult<TDest> TryMap(TSource sourceValue)
        {
            var destType = typeof (TDest);
            var mappingResult = Map(sourceValue, destType);

            return mappingResult.Success
                ? OperationResult<TDest>.Successful((TDest)mappingResult.Value)
                : OperationResult<TDest>.Failed(mappingResult.Error);
        }

        public TDest Map(TSource source)
        {
            var mapResult = TryMap(source);
            return mapResult.Success ? mapResult.Value : default(TDest);
        }
    }

    public class ConvertTypeMapper : ITypeMapper
    {
        public IOperationResult Map(object source, Type destType)
        {
            if (source == null)
                return OperationResult.Failed();

            if (destType.IsInstanceOfType(source)) return OperationResult.Successful(source);

            try
            {
                var convertedValue = Convert.ChangeType(source, destType);
                return OperationResult.Successful(convertedValue);
            }
            catch (InvalidCastException ex)
            {
                return OperationResult.Failed(ex);
            }
            catch (FormatException ex)
            {
                return OperationResult.Failed(ex);
            }
            catch (OverflowException ex)
            {
                return OperationResult.Failed(ex);
            }
        }

        public bool CanMap(object source, Type destType)
        {
            if (destType.IsInstanceOfType(source)) return true;
            if(source is IConvertible) return true;
            return TypeDescriptor.GetConverter(source.GetType()).CanConvertTo(destType);
        }
    }
}