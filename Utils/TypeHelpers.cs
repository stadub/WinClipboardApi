using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class TypeHelpers
    {
        public static object GetDefault(Type type)
        {
            //http://stackoverflow.com/questions/325426/programmatic-equivalent-of-defaulttype
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
