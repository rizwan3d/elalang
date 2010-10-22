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
		private const string ISCONSTRUCTOR = "isConstructor";
		private const string ISPRIVATE = "isPrivate";
		private const string GETVALUE = "getValue";
		private RuntimeValue value;
		private int moduleHandle;
		
		internal ElaVariableInfo(int moduleHandle, int address, string name, RuntimeValue value, bool immutable, bool priv, bool ctor)
		{
			Address = address;
			Name = name;
			IsImmutable = immutable;
			IsPrivate = priv;
			IsConstructor = ctor;
			this.moduleHandle = moduleHandle;
			this.value = value;
		}
		#endregion


		#region Ela Functions
		private sealed class GetValueFunction : ElaFunction
		{
			private ElaVariableInfo obj;

			internal GetValueFunction(ElaVariableInfo obj) : base(0)
			{
				this.obj = obj;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return obj.GetVariableValue();
			}
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
		
		
		public RuntimeValue GetVariableValue()
		{
			return value;
		}


		internal protected override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case ADDRESS: return new RuntimeValue(Address);
				case NAME: return new RuntimeValue(Name);
				case ISIMMUTABLE: return new RuntimeValue(IsImmutable);
				case ISPRIVATE: return new RuntimeValue(IsPrivate);
				case ISCONSTRUCTOR: return new RuntimeValue(IsConstructor);
				case GETVALUE: return new RuntimeValue(new GetValueFunction(this));
				default: return base.GetAttribute(name);
			}
		}
		#endregion


		#region Properties
		public string Name { get; private set; }

		public int Address { get; private set; }

		public bool IsImmutable { get; private set; }

		public bool IsPrivate { get; private set; }

		public bool IsConstructor { get; private set; }
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
