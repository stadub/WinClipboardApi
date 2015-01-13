using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    partial class ServiceLocator
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

            public void InjectProperty<TProp>(Expression<Func<TClass, TProp>> expression)
            {
                var propType = GetPropertyInfo(expression);
                type.PropertyInjections.Add(propType);
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
}
