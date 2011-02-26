using System;
using System.Collections.Generic;
using System.Linq;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public class ElaTypeInfo : ElaInfo, IEquatable<ElaTypeInfo>
	{
		#region Construction
		private const string TYPE = "type";
		private const string TYPEID = "typeId";
		private const string ISBYREF = "isByRef";

		private ElaObject obj;

		internal ElaTypeInfo(ElaObject obj) : this(3, obj)
		{

		}

		internal ElaTypeInfo(int size, ElaObject obj) : base(size)
		{
			this.obj = obj;

			AddField(0, TYPE, new ElaValue(TypeCode.GetShortForm()));
			AddField(1, TYPEID, new ElaValue((Int32)TypeCode));
			AddField(2, ISBYREF, new ElaValue(IsByRef));

		}
		#endregion

		
		#region Methods
		public static bool Equals(ElaTypeInfo lho, ElaTypeInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.TypeCode == rho.TypeCode;
		}


		public bool Equals(ElaTypeInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaTypeInfo);
		}


		public override int GetHashCode()
		{
			return (Int32)TypeCode;
		}
		#endregion


		#region Properties
		public ElaTypeCode TypeCode
		{
			get { return (ElaTypeCode)obj.TypeId; }
		}


		public bool IsByRef
		{
			get
			{
				return TypeCode != ElaTypeCode.Integer && TypeCode != ElaTypeCode.Char &&
					TypeCode != ElaTypeCode.Boolean && TypeCode != ElaTypeCode.Single;
			}
		}
		#endregion


		#region Operators
		public static bool operator ==(ElaTypeInfo lho, ElaTypeInfo rho)
		{
			return ElaTypeInfo.Equals(lho, rho);
		}


		public static bool operator !=(ElaTypeInfo lho, ElaTypeInfo rho)
		{
			return !ElaTypeInfo.Equals(lho, rho);
		}
		#endregion
	}
}
