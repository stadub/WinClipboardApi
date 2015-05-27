using System.Collections.Generic;

namespace Utils.TypeMapping.ValueResolvers
{
    class SourceKeyValPairResolver<TProp> : SourceMappingResolverBase
    {
        protected override bool IsMemberSuitable(BuilderMemberInfo memberInfo)
        {
            return true;
        }

        protected override ISourceInfo ResolveSourceValue(MappingMemberInfo memberInfo)
        {
            IDictionary<string, TProp> sourceValues=memberInfo.SourceInstance as IDictionary<string, TProp>;
            if(sourceValues==null)
                return null;

            TProp sourceProp;
            var sourceFound = sourceValues.TryGetValue(memberInfo.Name, out sourceProp);

            return !sourceFound ? null : SourceInfo.Create(sourceProp);;
        }
    }
}