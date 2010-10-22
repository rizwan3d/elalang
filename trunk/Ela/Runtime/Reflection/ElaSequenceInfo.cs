using System;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public sealed class ElaSequenceInfo : ElaTypeInfo, IEquatable<ElaSequenceInfo>
	{
		#region Construction
		private const string ISLAZY = "isLazy";		

		internal ElaSequenceInfo(bool isLazy) : base(ObjectType.Sequence)
		{
			IsLazy = isLazy;
		}
		#endregion


		#region Methods
		public static bool Equals(ElaSequenceInfo lho, ElaSequenceInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.IsLazy == rho.IsLazy;
		}


		public bool Equals(ElaSequenceInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaSequenceInfo);
		}


		public override int GetHashCode()
		{
			return (IsLazy ? 0 : 1);
		}


		internal protected override RuntimeValue GetAttribute(string name)
		{
			return name == ISLAZY ? new RuntimeValue(IsLazy) : base.GetAttribute(name);
		}
		#endregion


		#region Properties
		public bool IsLazy { get; private set; }
		#endregion


		#region Operators
		public static bool operator ==(ElaSequenceInfo lho, ElaSequenceInfo rho)
		{
			return ElaSequenceInfo.Equals(lho, rho);
		}


		public static bool operator !=(ElaSequenceInfo lho, ElaSequenceInfo rho)
		{
			return !ElaSequenceInfo.Equals(lho, rho);
		}
		#endregion
	}
}