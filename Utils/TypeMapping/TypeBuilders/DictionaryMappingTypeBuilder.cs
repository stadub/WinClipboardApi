using System.Collections.Generic;
using System.Reflection;
using Utils.TypeMapping;
using Utils.TypeMapping.TypeBuilders;
using Utils.TypeMapping.ValueResolvers;

namespace Utils
{
    public class DictionaryMappingTypeBuilder<TSource,TDest> : TypeBuilder<TDest>
    {

        public DictionaryMappingTypeBuilder(): base()
        {
            base.RegisterSourceResolver(new SourceKeyValPairResolver<TSource>());
        }

        public new TypeMapperContext<IDictionary<string, TSource>, TDest> Context
        {
            get { return (TypeMapperContext<IDictionary<string, TSource>, TDest>)base.Context; }
            set { base.Context = value; }
        }

        public override void CreateBuildingContext()
        {
            base.Context= new TypeMapperContext<IDictionary<string,TSource>, TDest>();
        }
        
        protected override OperationResult GetValue(ISourceMappingResolver sourceMappingResolver, PropertyInfo propertyInfo)
        {
            return sourceMappingResolver.ResolveSourceValue(propertyInfo, Context.Source);
        }

        protected override OperationResult GetValue(ISourceMappingResolver sourceMappingResolver, ParameterInfo parameterInfo)
        {
            return sourceMappingResolver.ResolveSourceValue(parameterInfo, Context.Source);
        }
    }
}