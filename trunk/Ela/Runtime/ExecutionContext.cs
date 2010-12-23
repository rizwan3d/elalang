using System;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime
{
	public sealed class ExecutionContext
	{
		#region Construction
		internal ExecutionContext()
		{

		}
		#endregion


		#region Methods
		public void InvalidFormat(string format, ElaValue value)
		{
			Fail(ElaRuntimeError.InvalidFormat, format, value.ToString(), value.DataType.GetShortForm());
		}


		public void DivideByZero(ElaValue value)
		{
			Fail(ElaRuntimeError.DivideByZero, value.ToString(), value.DataType.GetShortForm());
		}


		public void InvalidRightOperand(ElaValue left, ElaValue right, ElaTraits trait)
		{
			Fail(ElaRuntimeError.RightOperand, right.ToString(), right.DataType.GetShortForm(),
				left.ToString(), left.DataType.GetShortForm(), trait.ToString());
		}


		public void InvalidLeftOperand(ElaValue left, ElaValue right, ElaTraits trait)
		{
			Fail(ElaRuntimeError.LeftOperand, left.ToString(), left.DataType.GetShortForm(), 
				right.ToString(), right.DataType.GetShortForm(), trait.ToString());
		}


		public void ConversionFailed(ElaValue source, ObjectType target)
		{
			ConversionFailed(source, target, Strings.GetMessage("NotSupported"));
		}


		public void ConversionFailed(ElaValue source, ObjectType target, string reason)
		{
			Fail(ElaRuntimeError.ConversionFailed, source.ToString(), source.DataType.GetShortForm(), 
				target.GetShortForm(), reason);
		}


		public void InvalidIndexType(ObjectType type)
		{
			Fail(ElaRuntimeError.InvalidIndexType, type.GetShortForm());
		}


		public void IndexOutOfRange(ElaValue index, ElaValue obj)
		{
			Fail(ElaRuntimeError.IndexOutOfRange, index.ToString(), index.DataType.GetShortForm(),
				obj.ToString(), obj.DataType.GetShortForm());
		}


		public void InvalidType(ObjectType expected, ObjectType given)
		{
			Fail(ElaRuntimeError.InvalidType, expected.GetShortForm(), given.GetShortForm());
		}


		public void UnknownField(string field, ElaValue val)
		{
			Fail(ElaRuntimeError.UnknownField, field, val.ToString(), val.DataType.GetShortForm());
		}


		public void Fail(ElaRuntimeError error, params object[] args)
		{
			Fail(new ElaError(error, args));
		}


		public void Fail(string category, string message)
		{
			Fail(new ElaError(category, message));
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
