using System;
using System.Collections.Generic;
using System.Linq;
using Ela.Linking;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public sealed class ElaModuleInfo : ElaTypeInfo, IEquatable<ElaModuleInfo>
	{
		#region Construction
		private const string GETGLOBALS = "getGlobalVariables";
		private const string GETVARIANTS = "getVariants";
		private const string GETREFERENCES = "getReferences";
		private const string ASSEMBLY = "assembly";
		private const string HANDLE = "handle";
		private const string NAME = "name";
		private const string CODEBASE = "codeBase";
		private const string ISNATIVE = "isNative";
		private const string ISMAINMODULE = "isMainModule";

		private IEnumerable<ElaVariableInfo> variables;
		private IEnumerable<ElaVariantInfo> variants;
		private IEnumerable<ElaReferenceInfo> references;
		
		internal ElaModuleInfo(ElaMachine vm, int handle, string name, string codeBase, bool native,
			IEnumerable<ElaVariableInfo> variables, IEnumerable<ElaVariantInfo> variants,
			IEnumerable<ElaReferenceInfo> references) : base(ObjectType.Module)
		{
			Handle = handle;
			Name = name;
			CodeBase = codeBase;
			IsNative = native;
			Machine = vm;
			Assembly = vm != null ? new ElaAssemblyInfo(vm) : null;
			this.variables = variables;
			this.variants = variants;
			this.references = references;
		}
		#endregion


		#region Functions
		private sealed class GetVariablesFunction : ElaFunction
		{
			private ElaModuleInfo obj;

			internal GetVariablesFunction(ElaModuleInfo obj) : base(0)
			{
				this.obj = obj;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return new RuntimeValue(new ElaArray(obj.GetGlobalVariables()));
			}
		}


		private sealed class GetVariantsFunction : ElaFunction
		{
			private ElaModuleInfo obj;

			internal GetVariantsFunction(ElaModuleInfo obj) : base(0)
			{
				this.obj = obj;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return new RuntimeValue(new ElaArray(obj.GetVariants()));
			}
		}


		private sealed class GetReferencesFunction : ElaFunction
		{
			private ElaModuleInfo obj;

			internal GetReferencesFunction(ElaModuleInfo obj) : base(0)
			{
				this.obj = obj;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return new RuntimeValue(new ElaArray(obj.GetReferences()));
			}
		}
		#endregion

		
		#region Methods
		public static bool Equals(ElaModuleInfo lho, ElaModuleInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.Handle == rho.Handle;
		}


		public bool Equals(ElaModuleInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaModuleInfo);
		}


		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}


		public ElaVariableInfo[] GetGlobalVariables()
		{
			return variables.ToArray();
		}


		public ElaVariantInfo[] GetVariants()
		{
			return variants.ToArray();
		}


		public ElaReferenceInfo[] GetReferences()
		{
			return references.ToArray();
		}


		public override string ToString()
		{
			return Name;
		}


		internal protected override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case GETGLOBALS: return new RuntimeValue(new GetVariablesFunction(this));
				case GETVARIANTS: return new RuntimeValue(new GetVariantsFunction(this));
				case GETREFERENCES: return new RuntimeValue(new GetReferencesFunction(this));

				case ASSEMBLY: return new RuntimeValue(Assembly);
				case HANDLE: return new RuntimeValue(Handle);
				case NAME: return new RuntimeValue(Name);
				case CODEBASE: return new RuntimeValue(CodeBase);
				case ISNATIVE: return new RuntimeValue(IsNative);
				case ISMAINMODULE: return new RuntimeValue(IsMainModule);

				default: return base.GetAttribute(name);
			}
		}
		#endregion


		#region Properties
		public ElaAssemblyInfo Assembly { get; private set; }
		
		public int Handle { get; private set; }

		public string Name { get; private set; }
		
		public string CodeBase { get; private set; }

		public bool IsNative { get; private set; }

		public bool IsMainModule { get { return Handle == 0; } }

		internal ElaMachine Machine { get; private set; }
		#endregion


		#region Operators
		public static bool operator ==(ElaModuleInfo lho, ElaModuleInfo rho)
		{
			return ElaModuleInfo.Equals(lho, rho);
		}


		public static bool operator !=(ElaModuleInfo lho, ElaModuleInfo rho)
		{
			return !ElaModuleInfo.Equals(lho, rho);
		}
		#endregion
	}
}
