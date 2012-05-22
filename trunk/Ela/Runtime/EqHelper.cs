using System;
using System.Collections.Generic;

namespace Ela.Runtime
{
    internal static class EqHelper
    {
        internal static bool ListEquals(IList<ElaValue> left, IList<ElaValue> right, ExecutionContext ctx)
        {
            if (left.Count != right.Count)
                return false;

            for (var i = 0; i < left.Count; i++)
            {
                var l = left[i];
                var r = right[i];

                if (l.Ref == null && r.Ref == null)
                    continue;
                else if (l.Ref == null || r.Ref == null)
                    return false;

                var res = l.Equal(l, r, ctx);

                if (ctx.Failed || !res)
                    return false;
            }

            return true;
        }


        internal static bool ListEquals<T>(IList<T> left, IList<T> right) where T : IEquatable<T>
        {
            if (left.Count != right.Count)
                return false;

            for (var i = 0; i < left.Count; i++)
                if (!left[i].Equals(right[i]))
                    return false;

            return true;
        }
    }
}
