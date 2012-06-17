using System;
using System.Collections.Generic;
using Ela.Debug;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaError : ElaVariant
	{
		private readonly object[] args;
        private string message;
        
		internal ElaError(ElaRuntimeError code) : this(code, null, null, new object[0])
		{

		}

		internal ElaError(ElaRuntimeError code, params object[] args) : this(code, null, null, args)
		{

		}
        		
		internal ElaError(string customCode, string message) : this(ElaRuntimeError.UserCode, customCode, message, new object[0])
		{

		}
        
		private ElaError(ElaRuntimeError code, string customCode, string message, params object[] args) : 
			base(null, new ElaValue(ElaUnit.Instance))
		{
			Code = code;
			this.message = message;				
			this.args = args;
			base.Tag = code != ElaRuntimeError.UserCode ? code.ToString() : customCode;
			base.Value = new ElaValue(Message);
		}

		internal string Message
		{
			get
			{
				if (message == null && Code != ElaRuntimeError.UserCode)
					return message = Strings.GetError(Code, args);
				else if (message == null)
					return String.Empty;
				else
					return message;
			}
		}
        
		internal string FullMessage
		{
			get
			{
				return Code != ElaRuntimeError.UserCode ? Message :
					!String.IsNullOrEmpty(Tag) && !String.IsNullOrEmpty(Message) ?
					Tag + ": " + Message :
					!String.IsNullOrEmpty(Tag) ? Tag : Message;
			}
		}
        
		internal ElaRuntimeError Code { get; private set; }

		internal int CodeOffset { get; set; }

		internal int Module { get; set; }

		internal Stack<StackPoint> Stack { get; set; }
	}
}
