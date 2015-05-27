namespace Utils.TypeMapping.ValueResolvers
{
    class SourceTypeInjector : SourceMappingResolverBase
    {
        protected override bool IsMemberSuitable(BuilderMemberInfo memberInfo)
        {
            return true;
        }

        protected override ISourceInfo ResolveSourceValue(MappingMemberInfo memberInfo)
        {
            var sourceValue = memberInfo.SourceInstance;
            if (sourceValue==null)
                return null;
            if(memberInfo.Type==memberInfo.SourceType)
                return SourceInfo.Create(sourceValue);;

            return null;
        }
    }
}