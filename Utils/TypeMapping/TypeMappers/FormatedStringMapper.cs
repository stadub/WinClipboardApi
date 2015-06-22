using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Utils.Helpers;

namespace Utils.TypeMapping.TypeMappers
{
    public class FormatedStringMapper<TSource> : TypeMapperBase<TSource, string>, ITypeInfoMapper
    {

        public IOperationResult Map(ISourceInfo sourceInfo, Type destType)
        {
            if (!destType.IsAssignableFrom(typeof(string))) return OperationResult.Failed();

            return TryMap(sourceInfo) as IOperationResult<string>;
        }

        public IOperationResult TryMap(ISourceInfo sourceInfo)
        {
            if (sourceInfo.Attributes== null)
                return OperationResult<string>.Failed();
            var formated = sourceInfo.Attributes.FirstOrDefault(x => x is FormatedAttribute) as FormatedAttribute;
            if (formated != null)
            {
                //Debugger.Assert(() => formated.Format != null, "Formated attribute shoud provide format value.");
                var propertyConverter = (IFormattable)sourceInfo.Value;
                var formatedString = propertyConverter.ToString(formated.Format, CultureInfo.InvariantCulture);

                return OperationResult<string>.Successful(formatedString);
            }

            return OperationResult<string>.Failed();
        }

        public override bool CanMap(TSource source)
        {
            return source is IFormattable;
        }

        public override IOperationResult<string> TryMap(TSource source)
        {
            return (IOperationResult<string>) TryMap(new SourceInfo(source));
        }
    }
}
