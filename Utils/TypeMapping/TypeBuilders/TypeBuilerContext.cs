using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Utils.TypeMapping.PropertyMappers;
using Utils.TypeMapping.TypeMappers;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping.TypeBuilders
{
    public class TypeBuilerContext<T> : TypeBuilerContext
    {

        public T DestInstance
        {
            get { return (T) Instance; }
            set { Instance = value; }
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
        private Dictionary<KeyValuePair<string, string>, ITypeMapper> propertyTypeMappers;
        protected Stack<ITypeMapper> TypeMappers { get; private set; }
        protected Stack<IPropertyMapper> PropertyMappers { get; private set; }
        public IList<IPropertyMappingInfo> ResolvedProperties { get; private set; }
        public virtual object Instance { get; set; }
        public Type DestType { get; private set; }
        public ICollection<KeyValuePair<string, string>> IgnoreProperties { get; set; }

        public TypeBuilerContext(Type destType):this(destType,new Dictionary<PropertyInfo, ITypeMapper>())
        {
        }

        protected TypeBuilerContext(Type destType, Dictionary<PropertyInfo, ITypeMapper> propertyMappers)
        {
            TypeMappers= new Stack<ITypeMapper>();
            PropertyMappers=new Stack<IPropertyMapper>();
            IgnoreProperties = new HashSet<KeyValuePair<string, string>>();
            DestType = destType;

            ResolvedProperties = new List<IPropertyMappingInfo>();

            RegisterTypeMapper(new ConvertTypeMapper());
            RegisterTypeMapper(new FormatedStringMapper<object>());

            RegisterPropertyMapper(new PropertyMapper());
            RegisterPropertyMapper(new InitPropertyMapper());
            RegisterPropertyMapper(new InjectTypeMapper());
            RegisterPropertyMapper(new IgnorePropertyMapper());
            
            this.propertyTypeMappers = new Dictionary<KeyValuePair<string, string>, ITypeMapper>();
            propertyMappers.ForEach(pair =>
            {
                var key = BuilderUtils.GetKey(pair.Key);
                propertyTypeMappers.Add(key, pair.Value);
            });
        }

        public void AddIgnoreProperty(PropertyInfo propertyInfo)
        {
            var key = BuilderUtils.GetKey(propertyInfo);
            IgnoreProperties.Add(key);
        }

        protected void RegisterTypeMapper(ITypeMapper typeMapper)
        {
            TypeMappers.Push(typeMapper);
        }
        

        protected void RegisterPropertyMapper(IPropertyMapper propertyMapper)
        {
            PropertyMappers.Push(propertyMapper);
        }

        public MappingResult MapProperty(IPropertyMappingInfo propInfo, ISourceInfo memberInfo)
        {
            IOperationResult result = OperationResult.Successful(memberInfo.Value);

            var key = BuilderUtils.GetKey(propInfo);
            

            if (propertyTypeMappers.ContainsKey(key))
            {
                var mapper = propertyTypeMappers[key];
                
                result = MapperTryGetValue(propInfo, memberInfo, mapper);
            }
            if (!result.Success) return MappingResult.NotResolved;

            foreach (IPropertyMapper mapper in PropertyMappers)
            {
                var valueMappers = TypeMappers.ToArray();

                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < valueMappers.Length; i++)
                {
                    var typeMapper = valueMappers[i];


                    var mapResult = mapper.MapPropery(typeMapper, propInfo, result.Value, memberInfo.Attributes);
                    if (mapResult)
                    {
                        ResolvedProperties.Add(propInfo);
                        return MappingResult.Resolved;
                    }
                }
            }
            return MappingResult.NotResolved;
        }

        public virtual IPropertyMappingInfo GetPropertyMappingInfo(PropertyInfo propertyInfo)
        {
            return new PropertyMappingInfo(propertyInfo, Instance);
        }

        private IOperationResult MapperTryGetValue(IPropertyMappingInfo propertyInfo, ISourceInfo memberInfo, ITypeMapper mapper)
        {
            if (mapper.CanMap(memberInfo.Value, propertyInfo.Type))
                return mapper.Map(memberInfo.Value, propertyInfo.Type);
            return OperationResult.Failed();
        }
    }
}