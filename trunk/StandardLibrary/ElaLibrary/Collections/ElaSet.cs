using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
	public sealed class ElaSet : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		public static readonly ElaSet Empty = new ElaSet(AvlTree.Empty);

		internal ElaSet(AvlTree tree) : base(ElaTraits.Eq | ElaTraits.Show | ElaTraits.Len | ElaTraits.Fold | ElaTraits.Gen | ElaTraits.Cons)
		{
			Tree = tree;
		}
		#endregion


		#region Methods
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
			return FromEnumerable(seq.OfType<Object>().Select(o => ElaValue.FromObject(o)));
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
			return Tree.Enumerate().Select(e => e.Key).GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ElaValue>)this).GetEnumerator();
		}
		#endregion



		#region Traits
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


		protected override ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
		{
			var next = instance as ElaSet;

			if (next == null)
			{
				ctx.Fail("InvalidType", "Invalid type! Expected set.");
				return Default();
			}

			return new ElaValue(next.Add(value));
		}
		#endregion


		#region Properties
		internal AvlTree Tree { get; private set; }
		#endregion
	}
}
