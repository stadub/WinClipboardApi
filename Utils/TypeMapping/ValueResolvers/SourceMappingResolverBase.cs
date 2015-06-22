using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers
{
    public abstract class SourceMappingResolverBase : ISourceMappingResolver
    {

        public bool IsMemberSuitable(IPropertyMappingInfo mappingMember)
        {
            return IsMemberSuitable(new BuilderMemberInfo(mappingMember));
        }

        public bool IsMemberSuitable(ParameterInfo mappingMember)
        {
            return IsMemberSuitable(new BuilderMemberInfo(mappingMember));
        }

        public ISourceInfo ResolveSourceValue(IPropertyMappingInfo mappingMember, object source)
        {
            return ResolveSourceValue(new MappingMemberInfo(mappingMember, source));
        }

        public ISourceInfo ResolveSourceValue(ParameterInfo mappingMember,object source)
        {
            return ResolveSourceValue(new MappingMemberInfo(mappingMember, source));
        }

        protected abstract bool IsMemberSuitable(BuilderMemberInfo memberInfo);

        protected abstract ISourceInfo ResolveSourceValue(MappingMemberInfo memberInfo);
       
    }
}