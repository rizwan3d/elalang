using System;
using System.Collections;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaString : ElaIndexedObject, IEnumerable<Char>, IEnumerable<RuntimeValue>
	{
		#region Construction
		private string buffer;
		
		public ElaString(string value) : base(ObjectType.String)
		{
			this.buffer = value ?? String.Empty;
		}		
		#endregion


		#region Methods
		public IEnumerator<Char> GetEnumerator()
		{
			foreach (var c in buffer)
				yield return c;
		}


		IEnumerator<RuntimeValue> IEnumerable<RuntimeValue>.GetEnumerator()
		{
			foreach (var c in buffer)
				yield return new RuntimeValue(c);
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		protected internal override RuntimeValue GetValue(RuntimeValue key)
		{
			return key.Type == ElaMachine.INT && key.I4 < buffer.Length && key.I4 >= 0 ? 
				new RuntimeValue(buffer[key.I4]) : new	RuntimeValue(Invalid);
		}


		public override string ToString()
		{
			return buffer;
		}
		#endregion


		#region Operators
		public static implicit operator ElaString(string val)
		{
			return new ElaString(val);
		}


		public static implicit operator string(ElaString val)
		{
			return val.Value;
		}
		#endregion


		#region Properties
		public override int Length
		{
			get { return buffer.Length; }
		}


		public string Value
		{
			get { return buffer; }
		}
		#endregion
	}
}
