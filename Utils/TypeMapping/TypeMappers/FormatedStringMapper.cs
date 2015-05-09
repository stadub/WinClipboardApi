using System;
using System.Globalization;
using System.Reflection;
using Utils.Helpers;

namespace Utils.TypeMapping.TypeMappers
{
    public class FormatedStringMapper<TSource> : TypeMapperBase<TSource, string>
    {

        public override IOperationResult<string> TryMap(TSource source)
        {
            var sourceType = source.GetType();
            var formated = sourceType.GetCustomAttribute<FormatedAttribute>();
            if (formated != null)
            {
                Debugger.Assert(() => formated.Format != null,"Formated attribute shoud provide format value.");
                var propertyConverter = (IFormattable)source;
                var formatedString = propertyConverter.ToString(formated.Format, CultureInfo.InvariantCulture);

                return OperationResult<string>.Successful(formatedString);
            }

            return OperationResult<string>.Failed();
        }
    }
}
