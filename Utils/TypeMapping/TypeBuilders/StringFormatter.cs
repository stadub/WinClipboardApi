using System;
using System.Collections.Generic;
using Utils.TypeMapping.TypeBuilders;
using Utils.TypeMapping.TypeMappers;

namespace Utils.TypeMapping
{
    public class StringFormatter<TSource> : TypeMapperBase<TSource,string>
    {
        public StringFormatter()
        {
            DestType = typeof(string);
        }
        public bool LocatorInjectOnlyFlaggedProperties { get;set; }

        public override bool CanMap(TSource source)
        {
            return true;
        }

        public override IOperationResult<string> TryMap(TSource source)
        {
            var mapper = CreateTypeBuilder();

            mapper.CreateBuildingContext();
            mapper.InitBuildingContext();
            mapper.Context.Source = source;

            //String instance will not be created becase DestObject initalized by StringFormatBuilder constructor
            //mapper.CreateInstance(ctor, DestType.FullName);
            
            mapper.CallInitMethods();

            mapper.InjectTypeProperties();

            return OperationResult<string>.Successful(mapper.Context.DestInstance);
        }

        protected virtual StringFormatBuilder<TSource> CreateTypeBuilder()
        {
            var mapper = new StringFormatBuilder<TSource>();
            return mapper;
        }

        public Type DestType { get; private set; }

    }


}
