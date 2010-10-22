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
		private const string ISINDEXED = "isIndexed";
		private const string ISENUM = "isEnumerable";
		private const string ISNUMBER = "isNumeric";
		private const string ISTUPLE = "isTuple";
		
		internal ElaTypeInfo(ObjectType type)
		{
			TypeCode = type;
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


		internal protected override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case TYPE: return new RuntimeValue(TypeCode.GetShortForm());
				case TYPEID: return new RuntimeValue((Int32)TypeCode);
				case ISBYREF: return new RuntimeValue(IsByRef);
				case ISENUM: return new RuntimeValue(IsEnumerable);
				case ISNUMBER: return new RuntimeValue(IsNumeric);
				case ISTUPLE: return new RuntimeValue(IsTuple);			
				
				default: return base.GetAttribute(name);
			}
		}


		private void CheckObject(ElaObject obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");
			else if (obj.TypeId != (Int32)TypeCode)
				throw new ElaParameterTypeException(TypeCode, (ObjectType)obj.TypeId);
		}
		#endregion


		#region Properties
		public ObjectType TypeCode { get; private set; }


		public bool IsByRef
		{
			get
			{
				return TypeCode != ObjectType.Integer && TypeCode != ObjectType.Char &&
					TypeCode != ObjectType.Boolean && TypeCode != ObjectType.Single;
			}
		}


		public bool IsEnumerable
		{
			get 
			{ 
				return TypeCode == ObjectType.Array || TypeCode == ObjectType.Tuple ||
					TypeCode == ObjectType.Record || TypeCode == ObjectType.String  || 
					TypeCode == ObjectType.Sequence || TypeCode == ObjectType.List; 
			}
		}


		public bool IsNumeric
		{
			get 
			{ 
				return TypeCode == ObjectType.Integer || TypeCode == ObjectType.Single ||
					TypeCode == ObjectType.Double || TypeCode == ObjectType.Long; 
			}
		}


		public bool IsTuple
		{
			get { return TypeCode == ObjectType.Tuple || TypeCode == ObjectType.Record; }
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
