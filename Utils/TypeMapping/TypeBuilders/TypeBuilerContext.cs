using System;
using System.Collections.Generic;
using System.Reflection;
using Utils.TypeMapping.PropertyMappers;
using Utils.TypeMapping.TypeMappers;

namespace Utils.TypeMapping.TypeBuilders
{
    public class TypeBuilerContext<T> : TypeBuilerContext
    {

        public new T Instance
        {
            get { return (T) base.Instance; }
            set { base.Instance = value; }
        }

        public TypeBuilerContext() : base(typeof(T))
        {
        }

        protected TypeBuilerContext(Dictionary<PropertyInfo, ITypeMapper> propertyMappers)
            : base(typeof(T), propertyMappers)
        {
        }
    }

    public class TypeBuilerContext
    {
        private Dictionary<PropertyInfo, ITypeMapper> propertyTypeMappers;
        private Stack<ITypeMapper> typeMappers = new Stack<ITypeMapper>();
        private Stack<IPropertyMapper> propertyMappers = new Stack<IPropertyMapper>(); 
        public IList<PropertyInfo> ResolvedProperties { get; private set; }
        public virtual object Instance { get; set; }
        public Type DestType { get; private set; }
        public ICollection<KeyValuePair<string, string>> IgnoreProperties { get; set; }

        public TypeBuilerContext(Type destType):this(destType,new Dictionary<PropertyInfo, ITypeMapper>())
        {
        }

        protected TypeBuilerContext(Type destType, Dictionary<PropertyInfo, ITypeMapper> propertyMappers)
        {
            IgnoreProperties = new HashSet<KeyValuePair<string, string>>();
            DestType = destType;

            ResolvedProperties = new List<PropertyInfo>();

            RegisterTypeMapper(new ConverTypeMapper());
            RegisterTypeMapper(new FormatedStringMapper<object>());

            RegisterPropertyMapper(new PropertyMapper());
            RegisterPropertyMapper(new InitPropertyMapper());
            
            this.propertyTypeMappers = propertyMappers;
        }

        public void AddIgnoreProperty(PropertyInfo propertyInfo)
        {
            var key = BuilderUtils.GetKey(propertyInfo);
            IgnoreProperties.Add(key);
        }

        protected void RegisterTypeMapper(ITypeMapper typeMapper)
        {
            typeMappers.Push(typeMapper);
        }

        protected void RegisterPropertyMapper(IPropertyMapper propertyMapper)
        {
            propertyMappers.Push(propertyMapper);
        }

        public MappingResult MapProperty(PropertyInfo propertyInfo, object value)
        {

            if (propertyTypeMappers.ContainsKey(propertyInfo))
            {
                var mapper = propertyTypeMappers[propertyInfo];

                var conversionResult = mapper.Map(value, propertyInfo.PropertyType);
                if (conversionResult.Success)
                {
                    propertyInfo.SetValue(Instance, conversionResult.Value);
                    ResolvedProperties.Add(propertyInfo);
                    return MappingResult.Resolved;
                }

            }

            foreach (IPropertyMapper mapper in propertyMappers)
            {
                var valueMappers = typeMappers.ToArray();

                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < valueMappers.Length; i++)
                {
                    var typeMapper = valueMappers[i];

                    var result = mapper.MapPropery(typeMapper, propertyInfo, value, Instance);
                    if (result)
                    {
                        ResolvedProperties.Add(propertyInfo);
                        return MappingResult.Resolved;
                    }
                }
            }
            return MappingResult.NotResolved;
        }
    }
}