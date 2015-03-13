using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Utils.ServiceLocatorInfo
{
    public class LocatorRegistrationInfo<TClass>
    {
        private readonly TypeBuilder type;
        public Type InterfaceType { get; private set; }
        public Type ClassType { get; private set; }


        internal LocatorRegistrationInfo(TypeBuilder type)
        {
            this.type = type;
        }

        public void InjectPropertyValue<TProp>(Expression<Func<TClass, TProp>> expression, TProp value)
        {
            var propInfo = GetPropertyInfo(expression);
            type.PropertyValueResolvers.Add(new KeyValuePair<PropertyInfo, object>(propInfo, value));
        }
#if PropertyInjectionResolvers
        public void InjectPropertyType<TBindType>(Expression<Func<TClass, object>> expression)
        {
            var propInfo = GetPropertyInfo(expression);

            type.PropertyInjectionResolvers.Add(new KeyValuePair<PropertyInfo, Type>(propInfo, typeof(TBindType)));
        }
#endif
        public void InjectProperty<TProp>(Expression<Func<TClass, TProp>> expression)
        {
            InjectNamedProperty(expression, string.Empty);
        }

        public void InjectNamedProperty<TProp>(Expression<Func<TClass, TProp>> expression,string reristeredName)
        {
            var propInfo = GetPropertyInfo(expression);
            type.PropertyInjections.Add(new KeyValuePair<string, PropertyInfo>(reristeredName, propInfo));
        }

        private static PropertyInfo GetPropertyInfo(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    return GetPropertyInfo(((LambdaExpression)expression).Body);
                case ExpressionType.MemberAccess:
                    {
                        var ma = (MemberExpression)expression;
                        var prop = ma.Member as PropertyInfo;
                        return prop;
                    }
                default:
                    throw new ArgumentException("Only property expression is alowed", "expression");
            }
        }
    }
    
}
