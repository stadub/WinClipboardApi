using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers
{
    public abstract class SourceMappingResolverBase : ISourceMappingResolver
    {

        public bool IsMemberSuitable(PropertyInfo mappingMember)
        {
            return IsMemberSuitable(new BuilderMemberInfo(mappingMember));
        }

        public bool IsMemberSuitable(ParameterInfo mappingMember)
        {
            return IsMemberSuitable(new BuilderMemberInfo(mappingMember));
        }

        public OperationResult ResolveSourceValue(PropertyInfo mappingMember, object source)
        {
            return ResolveSourceValue(new MappingMemberInfo(mappingMember, source));
        }

        public OperationResult ResolveSourceValue(ParameterInfo mappingMember,object source)
        {
            return ResolveSourceValue(new MappingMemberInfo(mappingMember, source));
        }

        protected abstract bool IsMemberSuitable(BuilderMemberInfo memberInfo);

        protected abstract OperationResult ResolveSourceValue(MappingMemberInfo memberInfo);
       
    }
}