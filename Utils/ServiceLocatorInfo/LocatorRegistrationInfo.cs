using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Utils.ServiceLocatorInfo
{
        public class LocatorRegistrationInfo<TClass>
        {
            private readonly TypeRegistrationInfo type;
            public Type InterfaceType { get; private set; }
            public Type ClassType { get; private set; }

            protected internal LocatorRegistrationInfo(TypeRegistrationInfo type)
            {
                this.type = type;
            }

            public void InjectProperty<TProp>(Expression<Func<TClass, TProp>> expression,TProp value)
            {
                var propType = GetPropertyInfo(expression);
            }

            public void InjectPropertyType<TBindType>(Expression<Func<TClass, object>> expression)
            {
                var propInfo = GetPropertyInfo(expression);

                type.PropertyInjectionResolvers.Add(propInfo, typeof(TBindType));
            }

            public void InjectProperty<TProp>(Expression<Func<TClass, TProp>> expression)
            {
                var propInfo = GetPropertyInfo(expression);
                type.PropertyInjections.Add(propInfo);
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
