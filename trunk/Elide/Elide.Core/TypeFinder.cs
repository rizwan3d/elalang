using System;

namespace Elide.Core
{
    public static class TypeFinder
    {
        public static Type Get(string typeStr)
        {
            var type = Type.GetType(typeStr);

            if (type == null)
                throw new ElideException("Unable to find type '{0}'.", typeStr);

            return type;
        }
    }
}
