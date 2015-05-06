using System;
using System.Linq;

namespace Utils.TypeMapping.ValueResolvers.ServiceLocator
{
    public class LocatorValueInjector : SourceMappingResolverBase
    {
        private readonly Utils.ServiceLocator locator;
        public bool InjectOnlyFlaggedProperties { get; set; }

        public LocatorValueInjector(Utils.ServiceLocator locator)
        {
            this.locator = locator;
        }

        protected override bool IsMemberSuitable(BuilderMemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberType.Parameter || !InjectOnlyFlaggedProperties) 
                return true;
            return memberInfo.Attributes.Any(x => x is InjectInstanceAttribute || x is ShoudlInjectAttribute);
        }
        
        protected override OperationResult ResolveSourceValue(MappingMemberInfo memberInfo)
        {
            var injectInstanceAttribute = memberInfo.Attributes.FirstOrDefault(x => x is InjectInstanceAttribute) as InjectInstanceAttribute;

            if (!IsMemberSuitable(memberInfo))
                return OperationResult.Failed();

            object value;
            var result = locator.TryResolve(memberInfo.Type, injectInstanceAttribute != null ? injectInstanceAttribute.Name : String.Empty, out value);
            return result ? OperationResult.Successful(value) : OperationResult.Failed();
        }
    }
}
