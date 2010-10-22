using System;
using System.Linq;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public class ElaRecordInfo : ElaTupleInfo, IEquatable<ElaRecordInfo>
	{
		#region Construction
		private const string GETFIELD = "getField";
		private const string SETFIELD = "setField";
		private const string HASFIELD = "hasField";
		private const string GETFIELDS = "getFields";
		
		internal ElaRecordInfo(string tag) : base(tag, ObjectType.Record)
		{
			
		}
		#endregion


		#region Ela Functions
		private sealed class GetFieldFunction : ElaFunction
		{
			internal GetFieldFunction() : base(2)
			{
				
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				if (args[0].Type != ElaMachine.REC)
					throw new ElaParameterTypeException(ObjectType.Record, args[0].DataType);

				return ((ElaRecord)args[0].Ref).GetField(args[1].ToString());
			}
		}


		private sealed class SetFieldFunction : ElaFunction
		{
			internal SetFieldFunction() : base(3)
			{
				
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				if (args[0].Type != ElaMachine.REC)
					throw new ElaParameterTypeException(ObjectType.Record, args[0].DataType);

				return new RuntimeValue(((ElaRecord)args[0].Ref).SetField(args[1].ToString(), args[2]));
			}
		}


		private sealed class HasFieldFunction : ElaFunction
		{
			internal HasFieldFunction() : base(1)
			{
				
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				if (args[0].Type != ElaMachine.REC)
					throw new ElaParameterTypeException(ObjectType.Record, args[0].DataType);

				return new RuntimeValue(((ElaRecord)args[0].Ref).HasField(args[1].ToString()));
			}
		}


		private sealed class GetFieldsFunction : ElaFunction
		{
			internal GetFieldsFunction() : base(0)
			{
				
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				if (args[0].Type != ElaMachine.REC)
					throw new ElaParameterTypeException(ObjectType.Record, args[0].DataType);

				return new RuntimeValue(new ElaArray(((ElaRecord)args[0].Ref).GetKeys().ToArray()));
			}
		}
		#endregion


		#region Methods
		public static bool Equals(ElaRecordInfo lho, ElaRecordInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.Tag == rho.Tag;
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
			return Tag != null ? Tag.GetHashCode() : 0;
		}


		internal protected override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case GETFIELD: return new RuntimeValue(new GetFieldFunction());
				case SETFIELD: return new RuntimeValue(new SetFieldFunction());
				case HASFIELD: return new RuntimeValue(new HasFieldFunction());
				case GETFIELDS: return new RuntimeValue(new GetFieldsFunction());
				default: return base.GetAttribute(name);
			}
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