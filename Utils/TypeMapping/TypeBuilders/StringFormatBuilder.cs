using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Utils.TypeMapping.TypeMappers;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping.TypeBuilders
{
    public class StringFormatBuilder<TSource> : MappingTypeBuilder<TSource, string>
    {

        public override void InjectTypeProperties()
        {

            var resolvedProperties = Context.ResolvedProperties;

            //resolving injection properties, which wheren't registered in the "PropertyInjections"
            var propsToInjectValue = Context.SourceType.GetProperties()
                .Select(Context.GetPropertyMappingInfo)
                .Where(x => !resolvedProperties.Contains(x)).ToArray();

            foreach (var prop in propsToInjectValue)
            {
                var propKey = BuilderUtils.GetKey(prop);
                //mark ignored propertieswas as resolved
                if (Context.IgnoreProperties.Contains(propKey))
                {
                    resolvedProperties.Add(prop);
                    continue;
                }

                var resolutionResult = ResolvePropertyInfo(prop);
                if (resolutionResult != null)
                {
                    Context.MapProperty(prop, resolutionResult);
                }
            }
        }

        public override void InitBuildingContext()
        {
            base.InitBuildingContext();
        }

        public override void CreateBuildingContext()
        {
            base.Context = new StringFormatContext<TSource>();
        }

    }

    public class StringFormatContext<TSource> : TypeMapperContext<TSource, string>
    {
        private StringBuilder typeStringBuilder;

        public StringFormatContext()
        {
            typeStringBuilder = new StringBuilder();

            var mappers = TypeMappers.ToArray();
            TypeMappers.Clear();

            RegisterTypeMapper(new ToStringMapper<object>());
            mappers.Reverse().ForEach(RegisterTypeMapper);
        }

        public override object Instance
        {
            get { return typeStringBuilder.ToString(); }
            set { throw new NotSupportedException();}
        }

        public override IPropertyMappingInfo GetPropertyMappingInfo(PropertyInfo propertyInfo)
        {

            var info = new PropertyMappingInfo(propertyInfo, Instance);
            info.Type = typeof (string);
            info.ValueSetter = value =>
            {
                typeStringBuilder.AppendFormat("{0}:{1}", info.Name, value);
                typeStringBuilder.AppendLine();
            };
            return info;
        }

    }

  
}
