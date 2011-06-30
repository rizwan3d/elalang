using System;
using System.Collections;
using System.Collections.Generic;
using Ela.Debug;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaTuple : ElaObject, IEnumerable<ElaValue>
	{
		#region Construction
		private int cons;
		
		public ElaTuple(params object[] args) : base(ElaTypeCode.Tuple)
		{
			Values = new ElaValue[args.Length];

            for (var i = 0; i < args.Length; i++)
                Values[i] = ElaValue.FromObject(args[i]);

			cons = args.Length;
		}


		public ElaTuple(params ElaValue[] args) : base(ElaTypeCode.Tuple)
		{
			Values = args;
			cons = args.Length;
		}


		internal ElaTuple(int size) : this(size, ElaTypeCode.Tuple)
		{

		}

		
		internal ElaTuple(int size, ElaTypeCode type) : base(type)
		{
			Values = new ElaValue[size];
		}


		private ElaTuple() : base(ElaTypeCode.Tuple)
		{

		}
		#endregion


		#region Operations
		protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Ref == right.Ref)
				return new ElaValue(true);

			var lt = left.Ref as ElaTuple;
			var rt = right.Ref as ElaTuple;

			if (lt == null || rt == null || rt.Length != lt.Length)
				return new ElaValue(false);

			for (var i = 0; i < lt.Length; i++)
			{
				var fst = lt.Values[i].Id(ctx);
				var snd = rt.Values[i].Id(ctx);
				var eq = fst.Ref.Equal(fst, snd, ctx);

				if (!eq.Ref.Bool(eq, ctx))
					return new ElaValue(false);
			}
			
			return new ElaValue(true);
		}


		protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			if (left.Ref == right.Ref)
				return new ElaValue(false);

			var lt = left.Ref as ElaTuple;
			var rt = right.Ref as ElaTuple;

			if (lt == null || rt == null || rt.Length != lt.Length)
				return new ElaValue(true);

			for (var i = 0; i < lt.Length; i++)
			{
				var fst = lt.Values[i].Id(ctx);
				var snd = rt.Values[i].Id(ctx);
				var eq = fst.Ref.NotEqual(fst, snd, ctx);

				if (eq.Ref.Bool(eq, ctx))
					return new ElaValue(true);
			}

			return new ElaValue(false);
		}


		protected internal override ElaValue Greater(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);
			var rt = GetOther(lt, right, ctx);

			if (lt == null || rt == null || rt.Length != lt.Length)
				return Default();

			for (var i = 0; i < Length; i++)
			{
				var fst = lt.Values[i].Id(ctx);
				var snd = rt.Values[i].Id(ctx);
				var gt = fst.Ref.Greater(fst, snd, ctx);

				if (gt.Ref.Bool(gt, ctx))
					return new ElaValue(true);
			}

			return new ElaValue(false);
		}


		protected internal override ElaValue Lesser(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);
			var rt = GetOther(lt, right, ctx);

			if (lt == null || rt == null || rt.Length != lt.Length)
				return Default();

			for (var i = 0; i < Length; i++)
			{
				var fst = lt.Values[i].Id(ctx);
				var snd = rt.Values[i].Id(ctx);
				var clt = fst.Ref.Lesser(fst, snd, ctx);

				if (clt.Ref.Bool(clt, ctx))
					return new ElaValue(true);
			}

			return new ElaValue(false);
		}


		protected internal override ElaValue GreaterEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);
			var rt = GetOther(lt, right, ctx);

			if (lt == null || rt == null || rt.Length != lt.Length)
				return Default();

			for (var i = 0; i < Length; i++)
			{
				var fst = lt.Values[i].Id(ctx);
				var snd = rt.Values[i].Id(ctx);
				var gt = fst.Ref.GreaterEqual(fst, snd, ctx);

				if (gt.Ref.Bool(gt, ctx))
					return new ElaValue(true);
			}

			return new ElaValue(false);
		}


		protected internal override ElaValue LesserEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);
			var rt = GetOther(lt, right, ctx);

			if (lt == null || rt == null || rt.Length != lt.Length)
				return Default();

			for (var i = 0; i < Length; i++)
			{
				var fst = lt.Values[i].Id(ctx);
				var snd = rt.Values[i].Id(ctx);
				var lteq = fst.Ref.LesserEqual(fst, snd, ctx);

				if (lteq.Ref.Bool(lteq, ctx))
					return new ElaValue(true);
			}

			return new ElaValue(false);
		}


		protected internal override ElaValue Successor(ElaValue @this, ExecutionContext ctx)
		{
			var tuple = new ElaTuple(Length);
			tuple.cons = Length;

			for (var i = 0; i < Length; i++)
			{
				var left = Values[i].TypeId == ElaMachine.LAZ ?
					((ElaLazy)Values[i].Ref).Force() : Values[i];
				
				tuple.Values[i] = left.Ref.Successor(left, ctx);
			}
			
			return new ElaValue(tuple);
		}


		protected internal override ElaValue Predecessor(ElaValue @this, ExecutionContext ctx)
		{
			var tuple = new ElaTuple(Length);
			tuple.cons = Length;

			for (var i = 0; i < Length; i++)
			{
				var left = Values[i].TypeId == ElaMachine.LAZ ?
					((ElaLazy)Values[i].Ref).Force() : Values[i];
				
				tuple.Values[i] = left.Ref.Predecessor(left, ctx);
			}
			
			return new ElaValue(tuple);
		}


		protected internal override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);

			if (lt == null)
				return Default();
			
			if (right.TypeId != ElaMachine.TUP && right.TypeId != ElaMachine.REC)
			{
				var ret = new ElaTuple(Length);
				ret.cons = Length;
				
				for (var i = 0; i < Length; i++)
				{
					var val = lt.Values[i].Id(ctx);
					ret.Values[i] = val.Ref.Add(val, right, ctx);
				}

				return new ElaValue(ret);
			}
			else
			{
				var t = GetOther(lt, right, ctx);
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				if (t == null)
				{
					ctx.Fail(ElaRuntimeError.TuplesLength, left.ToString(), right.ToString());
					return Default();
				}

				for (var i = 0; i < Length; i++)
				{
					var fst = lt.Values[i].Id(ctx);
					var snd = t.Values[i].Id(ctx);
					ret.Values[i] = fst.Ref.Add(fst, snd, ctx);
				}

				return new ElaValue(ret);
			}
		}


		protected internal override ElaValue Subtract(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);

			if (lt == null)
				return Default();
			
			if (right.TypeId != ElaMachine.TUP && right.TypeId != ElaMachine.REC)
			{
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				for (var i = 0; i < Length; i++)
				{
					var val = lt.Values[i].Id(ctx);
					ret.Values[i] = val.Ref.Subtract(val, right, ctx);
				}

				return new ElaValue(ret);
			}
			else
			{
				var t = GetOther(lt, right, ctx);
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				if (t == null)
				{
					ctx.Fail(ElaRuntimeError.TuplesLength, ToString(), right.ToString());
					return Default();
				}

				for (var i = 0; i < Length; i++)
				{
					var fst = lt.Values[i].Id(ctx);
					var snd = t.Values[i].Id(ctx);
					ret.Values[i] = fst.Ref.Subtract(fst, snd, ctx);
				}

				return new ElaValue(ret);
			}
		}


		protected internal override ElaValue Multiply(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);

			if (lt == null)
				return Default();
			
			if (right.TypeId != ElaMachine.TUP && right.TypeId != ElaMachine.REC)
			{
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				for (var i = 0; i < Length; i++)
				{
					var val = lt.Values[i].Id(ctx);
					ret.Values[i] = val.Ref.Multiply(val, right, ctx);
				}

				return new ElaValue(ret);
			}
			else
			{
				var t = GetOther(lt, right, ctx);
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				if (t == null)
				{
					ctx.Fail(ElaRuntimeError.TuplesLength, ToString(), right.ToString());
					return Default();
				}

				for (var i = 0; i < Length; i++)
				{
					var fst = lt.Values[i].Id(ctx);
					var snd = t.Values[i].Id(ctx);
					ret.Values[i] = fst.Ref.Multiply(fst, snd, ctx);
				}

				return new ElaValue(ret);
			}
		}


		protected internal override ElaValue Divide(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);

			if (lt == null)
				return Default();
			
			if (right.TypeId != ElaMachine.TUP && right.TypeId != ElaMachine.REC)
			{
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				for (var i = 0; i < Length; i++)
				{
					var val = lt.Values[i].Id(ctx);
					ret.Values[i] = val.Ref.Divide(val, right, ctx);
				}

				return new ElaValue(ret);
			}
			else
			{
				var t = GetOther(lt, right, ctx);
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				if (t == null)
				{
					ctx.Fail(ElaRuntimeError.TuplesLength, ToString(), right.ToString());
					return Default();
				}

				for (var i = 0; i < Length; i++)
				{
					var fst = lt.Values[i].Id(ctx);
					var snd = t.Values[i].Id(ctx);
					ret.Values[i] = fst.Ref.Divide(fst, snd, ctx);
				}

				return new ElaValue(ret);
			}
		}


		protected internal override ElaValue Remainder(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);

			if (lt == null)
				return Default();
			
			if (right.TypeId != ElaMachine.TUP && right.TypeId != ElaMachine.REC)
			{
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				for (var i = 0; i < Length; i++)
				{
					var val = lt.Values[i].Id(ctx);
					ret.Values[i] = val.Ref.Remainder(val, right, ctx);
				}

				return new ElaValue(ret);
			}
			else
			{
				var t = GetOther(lt, right, ctx);
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				if (t == null)
				{
					ctx.Fail(ElaRuntimeError.TuplesLength, ToString(), right.ToString());
					return Default();
				}

				for (var i = 0; i < Length; i++)
				{
					var fst = lt.Values[i].Id(ctx);
					var snd = t.Values[i].Id(ctx);
					ret.Values[i] = fst.Ref.Remainder(fst, snd, ctx);
				}

				return new ElaValue(ret);
			}
		}


		protected internal override ElaValue Power(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			var lt = GetOther(null, left, ctx);

			if (lt == null)
				return Default();
			
			if (right.TypeId != ElaMachine.TUP && right.TypeId != ElaMachine.REC)
			{
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				for (var i = 0; i < Length; i++)
				{
					var val = lt.Values[i].Id(ctx);
					ret.Values[i] = val.Ref.Power(val, right, ctx);
				}

				return new ElaValue(ret);
			}
			else
			{
				var t = GetOther(lt, right, ctx);
				var ret = new ElaTuple(Length);
				ret.cons = Length;

				if (t == null)
				{
					ctx.Fail(ElaRuntimeError.TuplesLength, ToString(), right.ToString());
					return Default();
				}

				for (var i = 0; i < Length; i++)
				{
					var fst = lt.Values[i].Id(ctx);
					var snd = t.Values[i].Id(ctx);
					ret.Values[i] = fst.Ref.Power(fst, snd, ctx);
				}

				return new ElaValue(ret);
			}
		}


		protected internal override bool Bool(ElaValue @this, ExecutionContext ctx)
		{
			var ret = new ElaTuple(Length);
			ret.cons = Length;

			for (var i = 0; i < Length; i++)
			{
				var left = Values[i];

				if (!left.Ref.Bool(left, ctx))
					return false;
			}

			return true;
		}


		protected internal override ElaValue Negate(ElaValue @this, ExecutionContext ctx)
		{
			var ret = new ElaTuple(Length);
			ret.cons = Length;

			for (var i = 0; i < Length; i++)
			{
				var left = Values[i].TypeId == ElaMachine.LAZ ?
					((ElaLazy)Values[i].Ref).Force() : Values[i];

				ret.Values[i] = left.Ref.Negate(left, ctx);
			}

			return new ElaValue(ret);
		}


		protected internal override ElaValue Concatenate(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            if (left.TypeId != ElaMachine.TUP)
            {
                ctx.InvalidType(TypeCodeFormat.GetShortForm(ElaTypeCode.Tuple), left);
                return Default();
            }

            if (right.TypeId != ElaMachine.TUP)
            {
                ctx.InvalidType(TypeCodeFormat.GetShortForm(ElaTypeCode.Tuple), right);
                return Default();
            }

            var arr1 = ((ElaTuple)left.Ref).Values;
            var arr2 = ((ElaTuple)right.Ref).Values;
            var res = new ElaValue[arr1.Length + arr2.Length];
            arr1.CopyTo(res, 0);
            arr2.CopyTo(res, arr1.Length);
            return new ElaValue(new ElaTuple(res));
		}


		protected internal override ElaValue GetLength(ExecutionContext ctx)
		{
			return new ElaValue(Length);
		}


		protected internal override ElaValue GetValue(ElaValue key, ExecutionContext ctx)
		{
			if (key.TypeId != ElaMachine.INT)
			{
				ctx.InvalidIndexType(key);
				return Default();
			}

			if (key.I4 < Length && key.I4 > -1)
				return Values[key.I4];
			
			ctx.IndexOutOfRange(key, new ElaValue(this));
			return Default();
		}


		protected internal override ElaValue Convert(ElaValue @this, ElaTypeCode type, ExecutionContext ctx)
		{
			if (type == ElaTypeCode.Tuple)
				return @this;

			ctx.ConversionFailed(@this, type);
			return Default();
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return "(" + FormatHelper.FormatEnumerable((IEnumerable<ElaValue>)this, ctx, info) + 
				(Length < 2 ? ",)" : ")");
		}


		protected internal override ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			InternalSetValue(value);
			return new ElaValue(this);
		}


		protected internal override ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			return new ElaValue(this);
		}
		#endregion


		#region Methods
        public override ElaPatterns GetSupportedPatterns()
        {
            return ElaPatterns.Tuple;
        }


		internal static ElaTuple FromArray(ElaValue[] array)
		{
			var tup = new ElaTuple();
			tup.Values = array;
			tup.cons = array.Length;
			return tup;
		}


		internal ElaValue FastGet(int index)
		{
			return index < Length ? Values[index] : Default();
		}


		public IEnumerator<ElaValue> GetEnumerator()
		{
			for (var i = 0; i < Values.Length; i++)
                yield return Values[i];
		}


        IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		internal void InternalSetValue(ElaValue value)
		{
			if (value.TypeId == ElaMachine.TUP)
			{
				var other = (ElaTuple)value.Ref;
				var arr = new ElaValue[Length + other.Length + (Values.Length - Length - 1)];
				Array.Copy(Values, 0, arr, 0, Length);
				other.Values.CopyTo(arr, Length);
				Values = arr;
				cons += other.Length;
			}
			else
				InternalSetValue(Length, value);
		}


		internal void InternalSetValue(int index, ElaValue value)
		{
			if (Length == Values.Length)
			{
				var arr = new ElaValue[Length + 1];
				Values.CopyTo(arr, 0);
				Values = arr;
			}
			
			Values[index] = value;
			cons++;
		}


		private ElaTuple GetOther(ElaTuple @this, ElaValue other, ExecutionContext ctx)
		{
			var t = other.Ref as ElaTuple;

			if (t == null)
			{
				ctx.InvalidType(GetTypeName(), other);
				return null;
			}

			if (@this != null && t.Length != @this.Length)
			{
				ctx.Fail(ElaRuntimeError.TuplesLength, ToString(), other.ToString());
				return null;
			}

			return t;
		}
		#endregion


        #region Properties
        public ElaValue this[int index]
		{
			get
			{
				if (index < Length && index > -1)
					return Values[index];
				else
					throw new IndexOutOfRangeException();
			}
		}


		public int Length 
		{ 
			get { return cons; } 
		}
		
		
		internal ElaValue[] Values { get; private set; }
		#endregion
	}
}
