using System;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public sealed class ElaFunctionInfo : ElaTypeInfo, IEquatable<ElaFunctionInfo>
	{
		#region Construction
		private const string MODULE = "module";
		private const string HANDLE = "handle";
		private const string NAME = "name";
		private const string PARAMS = "parameters";
		private const string ISNATIVE = "isNative";
		private const string ISSTATIC = "isStatic";
		private const string ISGLOBAL = "isGlobal";
		private const string ISCURRIED = "isCurried";

		
		internal ElaFunctionInfo(ElaModuleInfo module, int handle, string name, int pars, bool isNative, bool isStatic, 
			bool isGlobal, bool isCurried) : base(ObjectType.Function)
		{
			DeclaringModule = module;
			Handle = handle;
			Name = name;
			Parameters = pars;
			IsNative = isNative;
			IsStatic = isStatic;
			IsGlobal = isGlobal;
			IsCurried = isCurried;
		}
		#endregion


		#region Methods
		public static bool Equals(ElaFunctionInfo lho, ElaFunctionInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.Name == rho.Name && lho.Parameters == rho.Parameters &&
				lho.IsNative == rho.IsNative && lho.IsStatic == rho.IsStatic &&
				lho.IsGlobal == rho.IsGlobal && lho.IsCurried == rho.IsCurried;
		}


		public bool Equals(ElaFunctionInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaFunctionInfo);
		}


		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
		
		
		internal protected override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case MODULE: return new RuntimeValue(DeclaringModule);
				case HANDLE: return new RuntimeValue(Handle);
				case NAME: return new RuntimeValue(Name);
				case PARAMS: return new RuntimeValue(Parameters);
				case ISNATIVE: return new RuntimeValue(IsNative);
				case ISSTATIC: return new RuntimeValue(IsStatic);
				case ISGLOBAL: return new RuntimeValue(IsGlobal);
				case ISCURRIED: return new RuntimeValue(IsCurried);
				default: return base.GetAttribute(name);
			}
		}
		#endregion


		#region Properties
		public ElaModuleInfo DeclaringModule { get; private set; }

		public int Handle { get; private set; }

		public string Name { get; private set; }

		public int Parameters { get; private set; }

		public bool IsNative { get; private set; }

		public bool IsStatic { get; private set; }

		public bool IsGlobal { get; private set; }
		
		public bool IsCurried { get; private set; }
		#endregion


		#region Operators
		public static bool operator ==(ElaFunctionInfo lho, ElaFunctionInfo rho)
		{
			return ElaFunctionInfo.Equals(lho, rho);
		}


		public static bool operator !=(ElaFunctionInfo lho, ElaFunctionInfo rho)
		{
			return !ElaFunctionInfo.Equals(lho, rho);
		}
		#endregion
	}
}