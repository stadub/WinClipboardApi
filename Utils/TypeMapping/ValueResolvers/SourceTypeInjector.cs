namespace Utils.TypeMapping.ValueResolvers
{
    class SourceTypeInjector : SourceMappingResolverBase
    {
        protected override bool IsMemberSuitable(BuilderMemberInfo memberInfo)
        {
            return true;
        }

        protected override OperationResult ResolveSourceValue(MappingMemberInfo memberInfo)
        {
            var sourceValue = memberInfo.SourceInstance;
            if (sourceValue==null)
                return OperationResult.Failed();
            if(memberInfo.Type==memberInfo.SourceType)
                return OperationResult.Successful(sourceValue);

            return OperationResult.Failed();
        }
    }
}