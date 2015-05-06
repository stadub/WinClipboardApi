using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping.MappingInfo
{
    public class PropertyRegistrationInfo<TClass> : IPropertyRegistrationInfo<TClass>
    {
        public IList<KeyValuePair<string, string>> IgnoredProperties { get; private set; }
        public PropertyValueResolver ValueResolver { get; private set; }
        public PropertyRegistrationInfo()
        {
            ValueResolver = new PropertyValueResolver();
            IgnoredProperties= new List<KeyValuePair<string, string>>();
        }

        public void InjectPropertyValue<TProp>(Expression<Func<TClass, TProp>> poperty, TProp value)
        {
            ValueResolver.RegisterInjection(poperty, value);
        }

        public void IgnoreProperty<TProp>(Expression<Func<TClass, TProp>> expression)
        {
            var propInfo = TypeHelpers.GetPropertyInfo(expression);
            IgnoredProperties.Add(BuilderUtils.GetKey(propInfo));
        }
    }
}