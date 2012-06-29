using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
	public sealed class ExecutionContext
	{
		#region Construction
		private const string DEF_CATEGORY = "Failure";

		public ExecutionContext()
		{

		}
		#endregion


		#region Methods
        public void NoOverload(string type, string function)
        {
            Fail(ElaRuntimeError.NoOverload, function, type);
        }


		public void InvalidFormat(string format, ElaValue value)
		{
			Fail(ElaRuntimeError.InvalidFormat, format, value.ToString(), value.GetTypeName());
		}


        public void UnableRead(ElaValue val, string str)
        {
            Fail(ElaRuntimeError.Read, ((ElaTypeInfo)val.Ref).ReflectedTypeName, str);
        }


		public void DivideByZero(ElaValue value)
		{
			Fail(ElaRuntimeError.DivideByZero, value.ToString(), value.GetTypeName());
		}


		public void InvalidOperand(ElaValue left, ElaValue right, string op)
		{
			Fail(ElaRuntimeError.InvalidOperand, right.ToString(), right.GetTypeName(),
				left.ToString(), left.GetTypeName(), op);
		}


		public void ConversionFailed(ElaValue source, string targetType)
		{
			ConversionFailed(source, targetType, Strings.GetMessage("NotSupported"));
		}


		public void ConversionFailed(ElaValue source, string targetType, string reason)
		{
			Fail(ElaRuntimeError.ConversionFailed, source.ToString(), source.GetTypeName(),
                targetType, reason);
		}


		public void InvalidIndexType(ElaValue index)
		{
			Fail(ElaRuntimeError.InvalidIndexType, index.GetTypeName());
		}


		public void IndexOutOfRange(ElaValue index, ElaValue obj)
		{
			Fail(ElaRuntimeError.IndexOutOfRange, index.ToString(), index.GetTypeName(),
				obj.ToString(), obj.GetTypeName());
		}


		public void InvalidType(string expected, ElaValue given)
		{
			Fail(ElaRuntimeError.InvalidType, expected, given.GetTypeName());
		}


		public void NoOperator(ElaValue value, string op)
		{
			Fail(ElaRuntimeError.InvalidOp, value, value.GetTypeName(), op);
		}


		public void Fail(ElaRuntimeError error, params object[] args)
		{
			Fail(new ElaError(error, args));
		}


		public void Fail(string category, string message)
		{
			Fail(new ElaError(category, message));
		}


		public void Fail(string message)
		{
			Fail(DEF_CATEGORY, message);
		}


		internal void Fail(ElaError err)
		{
			if (!Failed)
			{
				Failed = true;
				Error = err;
			}
		}


		internal void Reset()
		{
			Failed = false;
			Error = null;
		}


        public void SetDeffered(ElaFunction fun, int args)
        {
            Failed = true;
            Fun = fun;
            DefferedArgs = args;
        }
		#endregion


		#region Properties
		internal bool Failed { get; set; }

		internal ElaError Error { get; set; }

        internal int DefferedArgs;

		internal ElaLazy Thunk;

        internal ElaFunction Fun;

        internal string Tag;

        internal string OverloadFunction;
		#endregion
	}
}
