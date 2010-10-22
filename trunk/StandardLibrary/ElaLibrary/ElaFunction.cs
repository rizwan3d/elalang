using System;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary
{
	public abstract class ElaFunction<R> : ElaFunction
	{
		#region Construction
		protected ElaFunction() : base(0)
		{

		}
		#endregion


		#region Methods
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			return ValueConverter.ConvertToRuntimeValue(Call());
		}


		protected abstract R Call();
		#endregion
	}


	public abstract class ElaFunction<P,R> : ElaFunction
	{
		#region Construction
		protected ElaFunction() : base(1)
		{

		}
		#endregion


		#region Methods
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			var res = Call(ValueConverter.ConvertFromRuntimeValue<P>(args[0]));
			return ValueConverter.ConvertToRuntimeValue(res);
		}


		protected abstract R Call(P arg);
		#endregion
	}


	public abstract class ElaFunction<P1,P2,R> : ElaFunction
	{
		#region Construction
		protected ElaFunction() : base(2)
		{

		}
		#endregion


		#region Methods
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			var res = Call(
				ValueConverter.ConvertFromRuntimeValue<P1>(args[0]),
				ValueConverter.ConvertFromRuntimeValue<P2>(args[1]));
			return ValueConverter.ConvertToRuntimeValue(res);
		}


		protected abstract R Call(P1 arg1, P2 arg2);
		#endregion
	}


	public abstract class ElaFunction<P1,P2,P3,R> : ElaFunction
	{
		#region Construction
		protected ElaFunction() : base(3)
		{

		}
		#endregion


		#region Methods
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			var res = Call(
				ValueConverter.ConvertFromRuntimeValue<P1>(args[0]),
				ValueConverter.ConvertFromRuntimeValue<P2>(args[1]),
				ValueConverter.ConvertFromRuntimeValue<P3>(args[2]));
			return ValueConverter.ConvertToRuntimeValue(res);
		}


		protected abstract R Call(P1 arg1, P2 arg2, P3 arg3);
		#endregion
	}
}
