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
		private const string ISGLOBAL = "isGlobal";
		private const string ISPARTIAL = "isPartial";

		
		internal ElaFunctionInfo(ElaObject obj, ElaModuleInfo module, int handle, string name, int pars, bool isNative, 
			bool isGlobal, bool isPartial) : base(10, obj)
		{
			DeclaringModule = module;
			Handle = handle;
			Name = name;
			Parameters = pars;
			IsNative = isNative;
			IsGlobal = isGlobal;
			IsPartiallyApplied = isPartial;

			AddField(3, MODULE, DeclaringModule != null ? new ElaValue(DeclaringModule) : new ElaValue(ElaUnit.Instance));
			AddField(4, HANDLE, new ElaValue(Handle));
			AddField(5, NAME, new ElaValue(Name));
			AddField(6, PARAMS, new ElaValue(Parameters));
			AddField(7, ISNATIVE, new ElaValue(IsNative));
			AddField(8, ISGLOBAL, new ElaValue(IsGlobal));
			AddField(9, ISPARTIAL, new ElaValue(IsPartiallyApplied));
		}
		#endregion


		#region Methods
		public static bool Equals(ElaFunctionInfo lho, ElaFunctionInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.Name == rho.Name && lho.Parameters == rho.Parameters &&
				lho.IsNative == rho.IsNative && lho.IsGlobal == rho.IsGlobal && 
				lho.IsPartiallyApplied == rho.IsPartiallyApplied;
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
		#endregion


		#region Properties
		public ElaModuleInfo DeclaringModule { get; private set; }

		public int Handle { get; private set; }

		public string Name { get; private set; }

		public int Parameters { get; private set; }

		public bool IsNative { get; private set; }

		public bool IsGlobal { get; private set; }
		
		public bool IsPartiallyApplied { get; private set; }
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