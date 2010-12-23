using System;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public sealed class ElaVariableInfo : ElaInfo, IEquatable<ElaVariableInfo>
	{
		#region Construction
		private const string ADDRESS = "address";
		private const string NAME = "name";
		private const string ISIMMUTABLE = "isImmutable";
		private const string ISPRIVATE = "isPrivate";
		private const string VALUE = "value";
		private ElaValue value;
		private int moduleHandle;
		
		internal ElaVariableInfo(int moduleHandle, int address, string name, ElaValue value, bool immutable, bool priv) : base(5)
		{
			Address = address;
			Name = name;
			IsImmutable = immutable;
			IsPrivate = priv;
			this.moduleHandle = moduleHandle;
			this.value = value;


			AddField(0, ADDRESS, new ElaValue(Address));
			AddField(1, NAME,  new ElaValue(Name));
			AddField(2, ISIMMUTABLE, new ElaValue(IsImmutable));
			AddField(3, ISPRIVATE, new ElaValue(IsPrivate));
			AddField(4, VALUE, value);
		}
		#endregion


		#region Methods
		public static bool Equals(ElaVariableInfo lho, ElaVariableInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.Address == rho.Address && lho.moduleHandle == rho.moduleHandle;
		}


		public bool Equals(ElaVariableInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaVariableInfo);
		}


		public override int GetHashCode()
		{
			return moduleHandle;
		}
		
		
		public ElaValue GetVariableValue()
		{
			return value;
		}
		#endregion


		#region Properties
		public string Name { get; private set; }

		public int Address { get; private set; }

		public bool IsImmutable { get; private set; }

		public bool IsPrivate { get; private set; }
		#endregion


		#region Operators
		public static bool operator ==(ElaVariableInfo lho, ElaVariableInfo rho)
		{
			return Equals(lho, rho);
		}


		public static bool operator !=(ElaVariableInfo lho, ElaVariableInfo rho)
		{
			return !Equals(lho, rho);
		}
		#endregion
	}
}
