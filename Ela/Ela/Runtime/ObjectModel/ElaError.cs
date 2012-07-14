using System;
using System.Collections.Generic;
using Ela.Debug;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaError : ElaObject
	{
		private readonly object[] args;
        private string message;
        
		internal ElaError(ElaRuntimeError code) : this(code, null, new object[0])
		{

		}

		internal ElaError(ElaRuntimeError code, params object[] args) : this(code, null, args)
		{

		}
        		
		internal ElaError(string message) : this(ElaRuntimeError.UserCode, message, new object[0])
		{

		}
        
		private ElaError(ElaRuntimeError code, string message, params object[] args) : 
			base(ElaTypeCode.Object)
		{
			Code = code;
			this.message = message;				
			this.args = args;
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
        
		internal ElaRuntimeError Code { get; private set; }

		internal int CodeOffset { get; set; }

		internal int Module { get; set; }

		internal Stack<StackPoint> Stack { get; set; }
	}
}
