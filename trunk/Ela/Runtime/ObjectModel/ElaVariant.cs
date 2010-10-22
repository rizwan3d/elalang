using System;
using System.Collections;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaVariant : ElaArray
	{
		#region Construction
		public ElaVariant(int size, string cons) : base(size, ObjectType.Variant)
		{
			Cons = cons;
		}
		#endregion


		#region Methods
		public override bool SetValue(int index, RuntimeValue value)
		{
			return false;
		}


		internal void InternalSetValue(int index, RuntimeValue value)
		{
			base.SetValue(index, value);
		}
		#endregion


		#region Properties
		public string Cons { get; private set; }
		#endregion
	}
}
