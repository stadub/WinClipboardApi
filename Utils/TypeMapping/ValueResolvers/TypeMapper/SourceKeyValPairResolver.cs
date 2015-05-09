using System.Collections.Generic;

namespace Utils.TypeMapping.ValueResolvers
{
    class SourceKeyValPairResolver<TProp> : SourceMappingResolverBase
    {
        protected override bool IsMemberSuitable(BuilderMemberInfo memberInfo)
        {
            return true;
        }

        protected override OperationResult ResolveSourceValue(MappingMemberInfo memberInfo)
        {
            IDictionary<string, TProp> sourceValues=memberInfo.SourceInstance as IDictionary<string, TProp>;
            if(sourceValues==null)
                return OperationResult.Failed();

            TProp sourceProp;
            var sourceFound = sourceValues.TryGetValue(memberInfo.Name, out sourceProp);

            return !sourceFound ? OperationResult.Failed() : OperationResult.Successful(sourceProp);
        }
    }
}