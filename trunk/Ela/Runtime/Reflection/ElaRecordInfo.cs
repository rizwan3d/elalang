using System;
using System.Linq;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public class ElaRecordInfo : ElaTupleInfo, IEquatable<ElaRecordInfo>
	{
		#region Construction
		private const string FIELDS = "fields";
		
		internal ElaRecordInfo(ElaRecord obj) : base(4, obj)
		{
			AddField(3, FIELDS, new ElaValue(ElaList.FromEnumerable(obj.GetKeys().ToArray())));
		}
		#endregion


		#region Methods
		public static bool Equals(ElaRecordInfo lho, ElaRecordInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null);
		}


		public bool Equals(ElaRecordInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaRecordInfo);
		}


		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion


		#region Operators
		public static bool operator ==(ElaRecordInfo lho, ElaRecordInfo rho)
		{
			return ElaRecordInfo.Equals(lho, rho);
		}


		public static bool operator !=(ElaRecordInfo lho, ElaRecordInfo rho)
		{
			return !ElaRecordInfo.Equals(lho, rho);
		}
		#endregion
	}
}