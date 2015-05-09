using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utils.TypeMapping.MappingInfo;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping.TypeBuilders
{
    public class TypeBuilder<T> : TypeBuilder
    {
        private readonly PropertyRegistrationInfo<T> registrationInfo;

        public new TypeBuilerContext<T> Context
        {
            get { return (TypeBuilerContext<T>) base.Context; }
            set { base.Context = value; }
        }

        public TypeBuilder()
            : base(typeof(T))
        {
            registrationInfo = new PropertyRegistrationInfo<T>();
        }

        public TypeBuilder(PropertyRegistrationInfo<T> registrationInfo)
            : base(typeof(T))
        {
            this.registrationInfo = registrationInfo;
        }

        public IPropertyRegistrationInfo<T> RegistrationInfo
        {
            get { return registrationInfo; }
        }

        public override void InitBuildingContext()
        {
            registrationInfo.IgnoredProperties.ForEach(Context.IgnoreProperties.Add);
            RegisterSourceResolver(registrationInfo.ValueResolver);
        }

        public override void CreateBuildingContext()
        {
            base.Context = new TypeBuilerContext<T>();
        }
    }

    public abstract class TypeBuilder
    {
        public const string CtorParamName = "[ctor]";
        public Type DestType { get; private set; }
        public List<MethodInfo> InitMethods { get; private set; }

        public TypeBuilerContext Context { get; protected set; }

        protected TypeBuilder(Type destType)
        {
            DestType = destType;
            InitMethods = new List<MethodInfo>();

            RegisterSourceResolver(new InjectValueResolver());
            RegisterSourceResolver(new OptionalParameterResolver());
            RegisterSourceResolver(new SourceTypeInjector());

            //RegisterSourceResolver(new LocatorValueInjector(new ServiceLocator()));
            //PropertyRegistrationInfo
        }


        protected List<ISourceMappingResolver> sourceResolvers = new List<ISourceMappingResolver>();

        public void RegisterSourceResolver(ISourceMappingResolver typeMapper)
        {
            sourceResolvers.Add(typeMapper);
        }

        public void RegisterPriorSourceResolver(ISourceMappingResolver typeMapper)
        {
            sourceResolvers.Add(typeMapper);
        }

        private List<ISourceMappingResolver> propertyMapper = new List<ISourceMappingResolver>();
        protected void RegisterProperyMapper(ISourceMappingResolver typeMapper)
        {
            propertyMapper.Add(typeMapper);
        }


        public ConstructorInfo GetConstructor()
        {
            return TypeHelpers.GetConstructor(DestType);
        }

        public void CreateInstance(ConstructorInfo ctor, string destTypeName)
        {
            //resolving constructor parametrs
            var paramsDef = ctor.GetParameters();
            var paramValues = ResolvePramValues(paramsDef, CtorParamName, destTypeName);
            Context.Instance = ctor.Invoke(paramValues.ToArray());
        }

        protected object[] ResolvePramValues(IList<ParameterInfo> paramsDef, string methodName, string destTypeName)
        {
            if (paramsDef.Count == 0)
                Context.Instance = Activator.CreateInstance(Context.DestType);

            var paramValues = new List<object>();
            foreach (ParameterInfo paramDef in paramsDef)
            {
                var parametrType = paramDef.ParameterType;
                if (paramDef.IsOut || parametrType.IsByRef)
                {
                    throw new TypeNotSupportedException(destTypeName, "Constructors/Methods with Out and Ref Attributes are not supported");
                }

                var paramResolution = ResolveParameterValue(paramDef);
                if (paramResolution.Success)
                    paramValues.Add(paramResolution.Value);
                else
                    throw new TypeInitalationException(DestType.FullName, string.Format("Property {0} doesn't resolved", paramDef.Name));
            }
            return paramValues.ToArray();
        }

        private OperationResult ResolveParameterValue(ParameterInfo paramDef)
        {
            foreach (var sourceResolver in sourceResolvers)
            {
                if (sourceResolver.IsMemberSuitable(paramDef))
                {
                    var sourceValue = GetValue(sourceResolver, paramDef);
                    if (sourceValue.Success)
                    {
                        return sourceValue;
                    }
                }
            }
            return OperationResult.Failed();
        }



        protected static IEnumerable<PropertyInfo> GetPublicNotIndexedProperties(Type type)
        {
            //work only with public Not special and not Index properties
            IEnumerable<PropertyInfo> props = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.IsSpecialName && x.GetIndexParameters().Length == 0);
            return props;
        }

        public virtual void CreateBuildingContext()
        {
            Context = new TypeBuilerContext(DestType);
            InitBuildingContext();
        }

        public abstract void InitBuildingContext();

        public virtual void InjectTypeProperties()
        {
            var resolvedProperties = Context.ResolvedProperties;

            //resolving injection properties, that wheren't registered in the "PropertyInjections"
            var propsToInjectValue = DestType.GetProperties()
                .Where(x => !resolvedProperties.Contains(x)).ToArray();

            foreach (var prop in propsToInjectValue)
            {
                var propKey = BuilderUtils.GetKey(prop);
                //mark ignored propertieswas resolved
                if (Context.IgnoreProperties.Contains(propKey))
                {
                    resolvedProperties.Add(prop);
                    continue;
                }

                var resolutionResult = ResolvePropertyValue(prop);
                if (resolutionResult.Success)
                    Context.MapProperty(prop, resolutionResult.Value);
            }
        }

        private OperationResult ResolvePropertyValue(PropertyInfo propertyInfo)
        {
            foreach (var sourceResolver in sourceResolvers)
            {
                if (sourceResolver.IsMemberSuitable(propertyInfo))
                {
                    var sourceValue = GetValue(sourceResolver, propertyInfo);
                    if (sourceValue.Success && sourceValue.Value!=null)
                    {
                        return sourceValue;
                    }
                }
            }
            return OperationResult.Failed();
        }

        protected virtual OperationResult GetValue(ISourceMappingResolver sourceMappingResolver, PropertyInfo propertyInfo)
        {
            return sourceMappingResolver.ResolveSourceValue(propertyInfo, null);
        }

        protected virtual OperationResult GetValue(ISourceMappingResolver sourceMappingResolver, ParameterInfo propertyInfo)
        {
            return sourceMappingResolver.ResolveSourceValue(propertyInfo, null);
        }

        public void CallInitMethods()
        {
            foreach (var method in InitMethods)
            {
                var paramsDef = method.GetParameters();
                var paramValues = ResolvePramValues(paramsDef, method.Name, Context.DestType.FullName);
                method.Invoke(Context.Instance, paramValues.ToArray());
            }
        }
    }

    public enum MappingResult
    {
        NotResolved = 0,
        Resolved = 1
    }


}