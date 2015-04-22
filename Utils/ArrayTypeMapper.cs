using System;
using System.Collections.Generic;
using Utils.ServiceLocatorInfo;

namespace Utils
{
    public class ArrayTypeMapper<TSource, TDest> : ITypeMapper<IEnumerable<TSource>, IList<TDest>>, ITypeMapper
    {
        private readonly ITypeMapper<TSource, TDest> elementMapper;

        public ArrayTypeMapper(ITypeMapper<TSource, TDest> elementMapper)
        {
            this.elementMapper = elementMapper;
        }

        public IList<TDest> Map(IEnumerable<TSource> source)
        {
            var list= new List<TDest>();
            foreach (TSource sourceItem in source)
            {
                list.Add(elementMapper.Map(sourceItem));
            }
            return list.ToArray();
        }

        object ITypeMapper.Map(object source)
        {
            return Map((IEnumerable<TSource>)source);
        }
    }
}
