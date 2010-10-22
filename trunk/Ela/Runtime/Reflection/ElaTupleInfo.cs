using System;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public class ElaTupleInfo : ElaTypeInfo, IEquatable<ElaTupleInfo>
	{
		#region Construction
		private const string TAG = "Tag";

		internal ElaTupleInfo(string tag, ObjectType type) : base(type)
		{
			Tag = tag;
		}
		#endregion


		#region Methods
		public static bool Equals(ElaTupleInfo lho, ElaTupleInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.Tag == rho.Tag;
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
			return Tag != null ? Tag.GetHashCode() : 0;
		}


		internal protected override RuntimeValue GetAttribute(string name)
		{
			return name == TAG ? new RuntimeValue(Tag) : base.GetAttribute(name);
		}
		#endregion


		#region Properties
		public string Tag { get; private set; }
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