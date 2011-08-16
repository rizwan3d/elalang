using System;
using System.Collections;
using System.Collections.Generic;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
	public sealed class ElaSet : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		public static readonly ElaSet Empty = new ElaSet(AvlTree.Empty);
		private const string TAG = "set";
		
		internal ElaSet(AvlTree tree)
		{
			Tree = tree;
		}
		#endregion


		#region Methods
        public override string GetTag()
		{
			return TAG;
		}


		public static ElaSet FromEnumerable(IEnumerable<ElaValue> seq)
		{
			var set = ElaSet.Empty;

			foreach (var v in seq)
			{
				if (set.Tree.Search(v).IsEmpty)
					set = new ElaSet(set.Tree.Add(v, default(ElaValue)));
			}

			return set;
		}


		public static ElaSet FromEnumerable(IEnumerable seq)
		{
			var set = ElaSet.Empty;

            foreach (var o in seq)
            {
                var v = ElaValue.FromObject(o);

                if (set.Tree.Search(v).IsEmpty)
                    set = new ElaSet(set.Tree.Add(v, default(ElaValue)));
            }

            return set;
		}


		public ElaSet Add(ElaValue value)
		{
			if (Tree.Search(value).IsEmpty)
				return new ElaSet(Tree.Add(value, default(ElaValue)));
			else
				return this;
		}


		public ElaSet Remove(ElaValue value)
		{
			if (!Tree.Search(value).IsEmpty)
				return new ElaSet(Tree.Remove(value));
			else
				return this;
		}


		public bool Contains(ElaValue value)
		{
			return !Tree.Search(value).IsEmpty;
		}


		public IEnumerator<ElaValue> GetEnumerator()
		{
			foreach (var kv in Tree.Enumerate())
                yield return kv.Key;
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ElaValue>)this).GetEnumerator();
		}
		#endregion


		#region Operations
		public override string ToString()
		{
			return "set[" + ConvertToList().ToString() + "]";
		}


		internal ElaValue Head()
		{
			return Tree.Key;
		}


		internal ElaSet Tail()
		{
			return new ElaSet(Tree.Remove(Tree.Key));
		}


		internal bool IsNil()
		{
			return Tree.IsEmpty;
		}


		internal ElaSet Generate(ElaValue value)
		{
			if (!Tree.Search(value).IsEmpty)
				return this;
			else
				return new ElaSet(Tree.Add(value, default(ElaValue)));
		}


		internal ElaSet Cons(ElaSet next, ElaValue value)
		{
			return next.Add(value);
		}


        public ElaList ConvertToList()
        {
            return ElaList.FromEnumerable(this);
        }
		#endregion


		#region Properties
		internal AvlTree Tree { get; private set; }

        public int Length
        {
            get
            {
                var c = 0;

                foreach (var _ in Tree.Enumerate())
                    c++;

                return c;
            }
        }
		#endregion
	}
}
