using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
using System.Text;

namespace Ela.Library.Collections
{
    public sealed class SetModule : ForeignModule
    {
        #region Construction
        private static readonly ElaSet empty = new ElaSet(AvlTree.Empty);

        public SetModule()
        {

        }
        #endregion


        #region Nested Classes
        public sealed class ElaSet : ElaObject, IEnumerable<ElaValue>
        {
            #region Construction
            internal ElaSet(AvlTree tree) : base(ElaTraits.Eq|ElaTraits.Show|ElaTraits.Len|ElaTraits.Fold|ElaTraits.Gen)
            {
                Tree = tree;
            }
            #endregion


            #region Methods
            public IEnumerator<ElaValue> GetEnumerator()
            {
                return Tree.Enumerate().Select(e => e.Key).GetEnumerator();
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
                return "set[" + FormatHelper.FormatEnumerable(this, ctx, info) + "]";
            }


            protected override ElaValue GetLength(ExecutionContext ctx)
            {
                return new ElaValue(Tree.Enumerate().Count());
            }


            protected override ElaValue Head(ExecutionContext ctx)
            {
                return Tree.Key;
            }


            protected override ElaValue Tail(ExecutionContext ctx)
            {
                return new ElaValue(new ElaSet(Tree.Remove(Tree.Key)));
            }


            protected override bool IsNil(ExecutionContext ctx)
            {
                return Tree.IsEmpty;
            }


            protected override ElaValue Generate(ElaValue value, ExecutionContext ctx)
            {
                if (!Tree.Search(value).IsEmpty)
                    return new ElaValue(this);
                else
                    return new ElaValue(new ElaSet(Tree.Add(value, default(ElaValue))));
            }


            protected override ElaValue GenerateFinalize(ExecutionContext ctx)
            {
                return new ElaValue(this);
            }
            #endregion


            #region Properties
            internal AvlTree Tree { get; private set; }
            #endregion
        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add("empty", empty);
            Add<IEnumerable<ElaValue>,ElaSet>("set", Create);
            Add<ElaValue,ElaSet,ElaSet>("add", Add);
            Add<ElaValue,ElaSet,ElaSet>("remove", Remove);
            Add<ElaValue,ElaSet,Boolean>("contains", Contains);
            Add<ElaSet,ElaList>("toList", ToList);
        }


        public ElaSet Create(IEnumerable<ElaValue> list)
        {
            var set = empty;

            foreach (var v in list)
            {
                if (set.Tree.Search(v).IsEmpty)
                    set = new ElaSet(set.Tree.Add(v, default(ElaValue)));
            }

            return set;
        }


        public ElaSet Add(ElaValue value, ElaSet set)
        {
            if (set.Tree.Search(value).IsEmpty)
                return new ElaSet(set.Tree.Add(value, default(ElaValue)));
            else
                return set;
        }


        public ElaSet Remove(ElaValue value, ElaSet set)
        {
            if (!set.Tree.Search(value).IsEmpty)
                return new ElaSet(set.Tree.Remove(value));
            else
                return set;
        }


        public bool Contains(ElaValue value, ElaSet set)
        {
            return !set.Tree.Search(value).IsEmpty;
        }


        public ElaList ToList(ElaSet set)
        {
            return ElaList.FromEnumerable(set);
        }
        #endregion
    }
}