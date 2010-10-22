using System;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaError : ElaRecord
	{
		#region Construction
		private const string TOSTRING_FORMAT = "[error:{0}]";

		internal ElaError(ElaRuntimeError code, string message) : base(2)
		{
			AddField(0, "code", new RuntimeValue((Int32)code));
			AddField(1, "message", new RuntimeValue(message));
			Tag = code.ToString().Replace("Runtime_", String.Empty);
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return String.Format(TOSTRING_FORMAT, (Int32)Code);
		}


		public override bool SetField(string key, RuntimeValue value)
		{
			return false;
		}


		protected internal override bool SetValue(RuntimeValue index, RuntimeValue value)
		{
			return false;
		}
		#endregion


		#region Properties
		public ElaRuntimeError Code
		{
			get { return (ElaRuntimeError)base.GetField("code").I4; }
		}

		public string Message
		{
			get { return base.GetField("message").ToString(); }
		}

		internal override bool ReadOnly { get { return true; } }
		#endregion
	}
}
