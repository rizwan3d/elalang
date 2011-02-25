using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class ElaMap : ElaObject, IEnumerable<ElaValue>
    {
        #region Construction
        public static readonly ElaMap Empty = new ElaMap(AvlTree.Empty);

        public ElaMap() : this(AvlTree.Empty)
        {

        }

        internal ElaMap(AvlTree tree) : base(ElaTraits.Eq | ElaTraits.Show | ElaTraits.Get | ElaTraits.Len)
        {
            Tree = tree;
        }
        #endregion


        #region Methods
        public ElaMap Add(ElaValue key, ElaValue value)
        {
            return new ElaMap(Tree.Add(key, value));
        }


        public ElaMap Remove(ElaValue key)
        {
            return new ElaMap(Tree.Remove(key));
        }


        public bool Contains(ElaValue key)
        {
            return !Tree.Search(key).IsEmpty;
        }


        public IEnumerator<ElaValue> GetEnumerator()
        {
            return Tree.Enumerate().Select(e => e.Value).GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ElaValue>)this).GetEnumerator();
        }


        protected override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(left.ReferenceEquals(right));
        }


        protected override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            return new ElaValue(!left.ReferenceEquals(right));
        }


        protected override string Show(ExecutionContext ctx, ShowInfo info)
        {
            var sb = new StringBuilder();
            sb.Append("map");
            sb.Append('{');
            var c = 0;

            foreach (var e in Tree.Enumerate())
            {
                if (c++ > 0)
                    sb.Append(',');

                sb.Append(e.Key.Show(ctx, info));
                sb.Append('=');
                sb.Append(e.Value.Show(ctx, info));
            }

            sb.Append('}');
            return sb.ToString();
        }


        protected override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
        {
            var res = Tree.Search(index);

            if (res.IsEmpty)
            {
                ctx.IndexOutOfRange(index, new ElaValue(this));
                return Default();
            }

            return res.Value;
        }


        protected override ElaValue GetLength(ExecutionContext ctx)
        {
            return new ElaValue(Tree.Enumerate().Count());
        }
        #endregion


        #region Properties
        internal AvlTree Tree { get; private set; }

        public int Length
        {
            get { return Tree.Enumerate().Count(); }
        }
        #endregion
    }
		
}
