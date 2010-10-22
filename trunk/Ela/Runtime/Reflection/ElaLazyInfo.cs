using System;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.Reflection
{
	public sealed class ElaLazyInfo : ElaTypeInfo, IEquatable<ElaLazyInfo>
	{
		#region Construction
		private const string ISASYNC = "isAsync";
		private const string ISEVAL = "isEvaluated";


		internal ElaLazyInfo(bool isAsync, bool isEval) : base(ObjectType.Lazy)
		{
			IsAsync = isAsync;
			IsEvaluated = isEval;
		}
		#endregion


		#region Methods
		public static bool Equals(ElaLazyInfo lho, ElaLazyInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				lho.IsAsync == rho.IsAsync;
		}


		public bool Equals(ElaLazyInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaLazyInfo);
		}


		public override int GetHashCode()
		{
			return (IsAsync ? 0 : 1) + (IsEvaluated ? 2 : 3);
		}
		
		
		internal protected override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case ISASYNC: return new RuntimeValue(IsAsync);
				case ISEVAL: return new RuntimeValue(IsEvaluated);
				default: return base.GetAttribute(name);
			}
		}
		#endregion


		#region Properties
		public bool IsAsync { get; private set; }

		public bool IsEvaluated { get; private set; }
		#endregion


		#region Operators
		public static bool operator ==(ElaLazyInfo lho, ElaLazyInfo rho)
		{
			return ElaLazyInfo.Equals(lho, rho);
		}


		public static bool operator !=(ElaLazyInfo lho, ElaLazyInfo rho)
		{
			return !ElaLazyInfo.Equals(lho, rho);
		}
		#endregion
	}
}