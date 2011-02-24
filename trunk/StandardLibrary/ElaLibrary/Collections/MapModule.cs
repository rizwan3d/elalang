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
	public sealed class MapModule : ForeignModule
	{
		#region Construction
		private static readonly ElaMap empty = new ElaMap(AvlTree.Empty);

		public MapModule()
		{

		}
		#endregion


		#region Nested Classes
		public sealed class ElaMap : ElaObject, IEnumerable<ElaValue>
		{
			#region Construction
			internal ElaMap(AvlTree tree) : base(ElaTraits.Eq|ElaTraits.Show|ElaTraits.Get|ElaTraits.Len)
			{
				Tree = tree;
			}
			#endregion


			#region Methods
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
			#endregion
		}
		#endregion


		#region Methods
		public override void Initialize()
		{
			Add("empty", empty);
			Add<ElaRecord,ElaMap>("map", CreateMap);
			Add<ElaValue,ElaValue,ElaMap,ElaMap>("add", Add);
			Add<ElaValue,ElaMap,ElaMap>("remove", Remove);
			Add<ElaValue,ElaMap,Boolean>("contains", Contains);
			Add<ElaValue,ElaMap,ElaVariant>("get", GetValue);
		}


		public ElaMap CreateMap(ElaRecord rec)
		{
			var map = empty;

			foreach (var k in rec.GetKeys())
				map = new ElaMap(map.Tree.Add(new ElaValue(k), rec[k]));

			return map;
		}


		public ElaMap Add(ElaValue key, ElaValue value, ElaMap map)
		{
			return new ElaMap(map.Tree.Add(key, value));
		}


		public ElaMap Remove(ElaValue key, ElaMap map)
		{
			return new ElaMap(map.Tree.Remove(key));
		}


		public bool Contains(ElaValue key, ElaMap map)
		{
			return !map.Tree.Search(key).IsEmpty;
		}


		public ElaVariant GetValue(ElaValue key, ElaMap map)
		{
			if (map.Tree.IsEmpty)
				return ElaVariant.None();

			var res = map.Tree.Search(key);
			return res.IsEmpty ? ElaVariant.None() : ElaVariant.Some(res.Value);
		}
		#endregion
	}
}