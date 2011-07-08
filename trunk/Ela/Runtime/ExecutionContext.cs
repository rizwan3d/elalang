﻿using System;
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
        public void NoOverload(string tag, string function)
        {
            Fail(ElaRuntimeError.NoOverload, function, tag);
        }


		public void InvalidFormat(string format, ElaValue value)
		{
			Fail(ElaRuntimeError.InvalidFormat, format, value.ToString(), value.GetTag());
		}


		public void DivideByZero(ElaValue value)
		{
			Fail(ElaRuntimeError.DivideByZero, value.ToString(), value.GetTag());
		}


		public void InvalidRightOperand(ElaValue left, ElaValue right, string op)
		{
			Fail(ElaRuntimeError.RightOperand, right.ToString(), right.GetTag(),
				left.ToString(), left.GetTag(), op);
		}


		public void InvalidLeftOperand(ElaValue left, ElaValue right, string op)
		{
			Fail(ElaRuntimeError.LeftOperand, left.ToString(), left.GetTag(), 
				right.ToString(), right.GetTag(), op);
		}


		public void ConversionFailed(ElaValue source, ElaTypeCode target)
		{
			ConversionFailed(source, target, Strings.GetMessage("NotSupported"));
		}


		public void ConversionFailed(ElaValue source, ElaTypeCode target, string reason)
		{
			Fail(ElaRuntimeError.ConversionFailed, source.ToString(), source.GetTag(),
                TypeCodeFormat.GetShortForm(target), reason);
		}


		public void InvalidIndexType(ElaValue index)
		{
			Fail(ElaRuntimeError.InvalidIndexType, index.GetTag());
		}


		public void IndexOutOfRange(ElaValue index, ElaValue obj)
		{
			Fail(ElaRuntimeError.IndexOutOfRange, index.ToString(), index.GetTag(),
				obj.ToString(), obj.GetTag());
		}


		public void InvalidType(string expected, ElaValue given)
		{
			Fail(ElaRuntimeError.InvalidType, expected, given.GetTag());
		}


		public void UnknownField(string field, ElaValue val)
		{
			Fail(ElaRuntimeError.UnknownField, field, val.ToString(), val.GetTag());
		}


		public void NoOperator(ElaValue value, string op)
		{
			Fail(ElaRuntimeError.InvalidOp, value, value.GetTag(), op);
		}


        public void NoOperation(ElaValue left, ElaValue right, string op)
        {
            Fail(ElaRuntimeError.NoOperation, op, left, right);
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
        private bool _failed;
        internal bool Failed 
        {
            get { return _failed; }
            set
            {
                _failed = value;
            }
        }

		internal ElaError Error { get; set; }

		internal ElaLazy Thunk;

        internal ElaFunction Function;

        internal int ToPop;
		#endregion
	}
}
