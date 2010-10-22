using System;
using System.Threading;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaLazy : ElaObject
	{
		#region Construction
		private bool async;

		internal ElaLazy(ElaFunction function) : base(ObjectType.Lazy)
		{
			Function = function;
		}


		internal ElaLazy(Thread thread) : base(ObjectType.Lazy)
		{
			Thread = thread;
			async = true;
		}
		#endregion


		#region Methods
		public override ElaTypeInfo GetTypeInfo()
		{
			return new ElaLazyInfo(async, HasValue());
		}


		public bool HasValue()
		{
			return InternalValue.Ref != null;
		}


		public void Force()
		{
			if (Function != null)
				SetValue(Function.Call());
			else
			{
				var t = Thread;

				if (t != null)
					t.Join();
			}
		}


		internal void SetValue(RuntimeValue value)
		{
			if (InternalValue.DataType != ObjectType.None)
				throw new InvalidOperationException();

			InternalValue = value;
			Thread = null;
			Function = null;
		}
		#endregion


		#region Properties
		public object Value
		{
			get
			{
				var v = InternalValue;

				if (v.Ref == null)
					throw new ElaFatalException(Strings.GetMessage("LazyNoValue"));

				return v.ToObject();
			}
		}

		internal ElaFunction Function { get; private set; }

		internal Thread Thread { get; private set; }
		
		internal RuntimeValue InternalValue { get; private set; }
		#endregion
	}
}