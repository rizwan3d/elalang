using System;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public class ElaTupleInfo : ElaTypeInfo, IEquatable<ElaTupleInfo>
	{
		#region Construction
		private ElaTuple obj;

		internal ElaTupleInfo(ElaObject obj) : this(3, obj)
		{
			this.obj = (ElaTuple)obj;
		}

		internal ElaTupleInfo(int size, ElaObject obj) : base(size, obj)
		{
			
		}
		#endregion


		#region Methods
		public static bool Equals(ElaTupleInfo lho, ElaTupleInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null);
		}


		public bool Equals(ElaTupleInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaTupleInfo);
		}


		public override int GetHashCode()
		{
			return obj.Length;
		}
		#endregion


		#region Operators
		public static bool operator ==(ElaTupleInfo lho, ElaTupleInfo rho)
		{
			return ElaTupleInfo.Equals(lho, rho);
		}


		public static bool operator !=(ElaTupleInfo lho, ElaTupleInfo rho)
		{
			return !ElaTupleInfo.Equals(lho, rho);
		}
		#endregion
	}
}