using System;
using System.Collections.Generic;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public abstract class ElaObject
	{
		#region Construction
		public static readonly ElaObject Unit = new SingletonObject(ObjectType.Unit);
		internal static readonly ElaObject Invalid = new SingletonObject(ObjectType.None);
		internal static readonly ElaObject Integer = new SingletonObject(ObjectType.Integer);
		internal static readonly ElaObject Single = new SingletonObject(ObjectType.Single);
		internal static readonly ElaObject Char = new SingletonObject(ObjectType.Char);
		internal static readonly ElaObject Boolean = new SingletonObject(ObjectType.Boolean);
		internal static readonly ElaObject Pointer = new SingletonObject((ObjectType)(ElaMachine.PTR));

		private const string GETTYPE = "getType";
		private const string TOSTRING_FORMAT = "[{0}]";

		
		protected ElaObject() : this(ObjectType.Object)
		{

		}

		
		internal ElaObject(ObjectType type)
		{
			TypeId = (Int32)type;
		} 
		#endregion


		#region Nested Classes
		private sealed class SingletonObject : ElaObject
		{
			internal SingletonObject(ObjectType type) : base(type) 
			{
			
			}

			public override string ToString()
			{
				return ((ObjectType)TypeId).GetShortForm();
			}
		}


		private sealed class GetTypeFunction : ElaFunction
		{
			private ElaObject obj;

			internal GetTypeFunction(ElaObject obj) : base(0)
			{
				this.obj = obj;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return new RuntimeValue(obj.GetTypeInfo());
			}
		}
		#endregion


		#region Methods
		public virtual ElaTypeInfo GetTypeInfo()
		{
			return new ElaTypeInfo((ObjectType)TypeId);
		}


		public override string ToString()
		{
			return String.Format(TOSTRING_FORMAT, ((ObjectType)TypeId).GetShortForm());
		}


		protected internal virtual RuntimeValue GetAttribute(string name)
		{
			return name == GETTYPE ? new RuntimeValue(new GetTypeFunction(this)) : new RuntimeValue(Invalid);
		}
		#endregion


		#region Properties
		internal int TypeId { get; private set; }
		#endregion
	}
}
