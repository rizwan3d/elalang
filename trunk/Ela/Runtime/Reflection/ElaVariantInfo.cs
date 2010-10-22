using System;
using System.Collections.Generic;

namespace Ela.Runtime.Reflection
{
	public sealed class ElaVariantInfo : ElaInfo, IEquatable<ElaVariantInfo>
	{
		#region Construction
		private const string NAME = "name";
		private const string PARAMS = "parameters";
		private const string HANDLE = "handle";

		internal ElaVariantInfo(string name, int handle)
		{
			Handle = handle;

			if (!String.IsNullOrEmpty(name))
			{
				var idx = name.IndexOf('@');

				if (idx != -1)
				{
					Name = name.Substring(0, idx);
					var p = 0;
					Int32.TryParse(name.Substring(idx + 1), out p);
					Parameters = p;
				}
				else
					Name = name;
			}
		}
		#endregion
		

		#region Methods
		public static bool Equals(ElaVariantInfo lho, ElaVariantInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.Name == rho.Name && lho.Parameters == rho.Parameters;
		}


		public bool Equals(ElaVariantInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaVariantInfo);
		}


		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		
		internal protected override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case NAME: return new RuntimeValue(Name);
				case HANDLE: return new RuntimeValue(Handle);
				case PARAMS: return new RuntimeValue(Parameters);
				default: return base.GetAttribute(name);
			}
		}
		#endregion


		#region Properties
		public string Name { get; private set; }

		public int Parameters { get; private set; }

		public int Handle { get; private set; }
		#endregion


		#region Operators
		public static bool operator ==(ElaVariantInfo lho, ElaVariantInfo rho)
		{
			return Equals(lho, rho);
		}


		public static bool operator !=(ElaVariantInfo lho, ElaVariantInfo rho)
		{
			return !Equals(lho, rho);
		}
		#endregion
	}
}
