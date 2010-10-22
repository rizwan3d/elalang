using System;
using System.Collections.Generic;

namespace Ela.Runtime.Reflection
{
	public sealed class ElaReferenceInfo : ElaInfo, IEquatable<ElaReferenceInfo>
	{
		#region Construction
		private const string MODULENAME = "moduleName";
		private const string DLLNAME = "dllName";
		private const string ALIAS = "alias";
		private const string FOLDER = "folder";
		
		internal ElaReferenceInfo(string moduleName, string dllName, string alias, string folder)
		{
			ModuleName = moduleName;
			DllName = dllName;
			Alias = alias;
			Folder = folder;
		}
		#endregion

		
		#region Methods
		public static bool Equals(ElaReferenceInfo lho, ElaReferenceInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.ModuleName == rho.ModuleName && lho.Folder == rho.Folder &&
				lho.DllName == rho.DllName && lho.Alias == rho.Alias;
		}


		public bool Equals(ElaReferenceInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaReferenceInfo);
		}


		public override int GetHashCode()
		{
			return ModuleName.GetHashCode();
		}
		
		
		internal protected override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case MODULENAME: return new RuntimeValue(ModuleName);
				case DLLNAME: return new RuntimeValue(DllName);
				case ALIAS: return new RuntimeValue(Alias);
				case FOLDER: return new RuntimeValue(Folder);
				default: return base.GetAttribute(name);
			}
		}
		#endregion


		#region Properties
		public string ModuleName { get; private set; }

		public string Alias { get; private set; }

		public string DllName { get; private set; }

		public string Folder { get; private set; }
		#endregion


		#region Operators
		public static bool operator ==(ElaReferenceInfo lho, ElaReferenceInfo rho)
		{
			return Equals(lho, rho);
		}


		public static bool operator !=(ElaReferenceInfo lho, ElaReferenceInfo rho)
		{
			return !Equals(lho, rho);
		}
		#endregion
	}
}
