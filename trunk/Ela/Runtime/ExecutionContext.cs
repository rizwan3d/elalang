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
		public void InvalidFormat(string format, ElaValue value)
		{
			Fail(ElaRuntimeError.InvalidFormat, format, value.ToString(), value.GetTypeName());
		}


		public void DivideByZero(ElaValue value)
		{
			Fail(ElaRuntimeError.DivideByZero, value.ToString(), value.GetTypeName());
		}


		public void InvalidRightOperand(ElaValue left, ElaValue right, string op)
		{
			Fail(ElaRuntimeError.RightOperand, right.ToString(), right.GetTypeName(),
				left.ToString(), left.GetTypeName(), op);
		}


		public void InvalidLeftOperand(ElaValue left, ElaValue right, string op)
		{
			Fail(ElaRuntimeError.LeftOperand, left.ToString(), left.GetTypeName(), 
				right.ToString(), right.GetTypeName(), op);
		}


		public void ConversionFailed(ElaValue source, ElaTypeCode target)
		{
			ConversionFailed(source, target, Strings.GetMessage("NotSupported"));
		}


		public void ConversionFailed(ElaValue source, ElaTypeCode target, string reason)
		{
			Fail(ElaRuntimeError.ConversionFailed, source.ToString(), source.GetTypeName(),
                TypeCodeFormat.GetShortForm(target), reason);
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


		public void UnknownField(string field, ElaValue val)
		{
			Fail(ElaRuntimeError.UnknownField, field, val.ToString(), val.GetTypeName());
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


		public void Fail(ElaError err)
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
		#endregion


		#region Properties
		internal bool Failed { get; set; }

		internal ElaError Error { get; set; }
		#endregion
	}
}
