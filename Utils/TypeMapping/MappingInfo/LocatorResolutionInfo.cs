using System;
using System.Linq.Expressions;

namespace Utils.TypeMapping.MappingInfo
{
    public class LocatorResolutionInfo<TClass> : ILocatorResolutionInfo<TClass>
    {
        private readonly LocatorRegistrationInfo<TClass> locatorRegistrationInfo;
        private readonly PropertyRegistrationInfo<TClass> propertyRegistrationInfo;

        public LocatorResolutionInfo(LocatorRegistrationInfo<TClass> locatorRegistrationInfo,PropertyRegistrationInfo<TClass> propertyRegistrationInfo)
        {
            this.locatorRegistrationInfo = locatorRegistrationInfo;
            this.propertyRegistrationInfo = propertyRegistrationInfo;
        }

        public void InjectProperty<TProp>(Expression<Func<TClass, TProp>> poperty)
        {
            locatorRegistrationInfo.InjectProperty(poperty);
        }

        public void InjectNamedProperty<TProp>(Expression<Func<TClass, TProp>> poperty, string reristeredName)
        {
            locatorRegistrationInfo.InjectNamedProperty(poperty, reristeredName);
        }

        public void InjectPropertyValue<TProp>(Expression<Func<TClass, TProp>> poperty, TProp value)
        {
            propertyRegistrationInfo.InjectPropertyValue(poperty, value);
        }

        public void IgnoreProperty<TProp>(Expression<Func<TClass, TProp>> poperty)
        {
            propertyRegistrationInfo.IgnoreProperty(poperty);
        }
    }
}