using System;
using System.Collections.Generic;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public abstract class ElaIndexedObject : ElaObject
	{
		#region Construction
		private const string LENGTH = "length";
		
		protected ElaIndexedObject(ObjectType type) : base(type)
		{

		}
		#endregion


		#region Methods
		protected internal override RuntimeValue GetAttribute(string name)
		{
			return name == LENGTH ? new RuntimeValue(Length) : base.GetAttribute(name);
		}


		protected internal abstract RuntimeValue GetValue(RuntimeValue index);


		protected internal virtual bool SetValue(RuntimeValue index, RuntimeValue value)
		{
			return false;
		}
		#endregion


		#region Properties
		internal virtual bool ReadOnly { get { return true; } }

		public abstract int Length { get; }
		#endregion
	}
}