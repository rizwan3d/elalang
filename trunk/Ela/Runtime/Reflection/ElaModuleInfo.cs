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
		private const string GLOBALS = "globals";
		private const string REFERENCES = "references";
		private const string ASSEMBLY = "assembly";
		private const string HANDLE = "handle";
		private const string NAME = "name";
		private const string CODEBASE = "codeBase";
		private const string ISNATIVE = "isNative";
		private const string ISMAINMODULE = "isMainModule";

		private IEnumerable<ElaVariableInfo> variables;
		private IEnumerable<ElaReferenceInfo> references;

		internal ElaModuleInfo(ElaObject obj, ElaMachine vm, int handle, string name, string codeBase, bool native,
			IEnumerable<ElaVariableInfo> variables, IEnumerable<ElaReferenceInfo> references)
			: base(11, obj)
		{
			Handle = handle;
			Name = name;
			CodeBase = codeBase;
			IsNative = native;
			Machine = vm;
			Assembly = vm != null ? new ElaAssemblyInfo(vm) : null;
			this.variables = variables;
			this.references = references;

			AddField(3, GLOBALS, new ElaValue(new ElaArray(GetGlobalVariables())));
			AddField(4, REFERENCES, new ElaValue(new ElaArray(GetReferences())));
			AddField(5, ASSEMBLY, new ElaValue(Assembly));
			AddField(6, HANDLE, new ElaValue(Handle));
			AddField(7, NAME, new ElaValue(Name));
			AddField(8, CODEBASE, new ElaValue(CodeBase));
			AddField(9, ISNATIVE, new ElaValue(IsNative));
			AddField(10, ISMAINMODULE, new ElaValue(IsMainModule));
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


		public ElaReferenceInfo[] GetReferences()
		{
			return references.ToArray();
		}


		public override string ToString()
		{
			return Name;
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
