using System;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public abstract class ElaObject : IEquatable<ElaValue>
	{
		#region Construction
		internal static readonly ExecutionContext DummyContext = new ExecutionContext();
		internal const string UNDEF = "<unknown>";
        internal const string INVALID = "<INVALID>";
        private const string TAG = "tag";
        private const string TYPEID = "typeId";
        private const string ISBYREF = "byRef";

		protected ElaObject(TypeId typeId)
		{
            if (typeId == null)
                throw new ElaTypeException(GetTag());
				
			TypeId = typeId.Id;
		}

		
		internal ElaObject(ElaTypeCode type)
		{
			TypeId = (Int32)type;
		} 
		#endregion


        #region Nested Classes
        internal sealed class ElaInvalidObject : ElaObject
        {
            internal static readonly ElaInvalidObject Instance = new ElaInvalidObject();

            internal ElaInvalidObject() : base(ElaTypeCode.Object)
			{ 
			
			}

            public override string ToString()
            {
                return INVALID;
            }
        }
        #endregion


        #region Methods
        internal virtual ElaValue Convert(ElaValue @this, ElaTypeCode typeCode)
        {
            if (typeCode == @this.TypeCode)
                return @this;
            else
                throw new InvalidCastException(String.Format("Unable to cast from {0} to {1}",
                    TypeCodeFormat.GetShortForm(@this.TypeCode), TypeCodeFormat.GetShortForm(typeCode)));
        }


        public virtual bool Equals(ElaValue other)
        {
            return this == other.Ref;
        }


        public virtual string GetTag()
        {
            return null;
        }

        internal virtual bool IsEvaluated()
        {
            return true;
        }

        internal virtual int GetLength(ExecutionContext ctx)
        {
            return 0;
        }


        protected internal virtual bool Is<T>(ElaValue value) where T : ElaObject
        {
            return this is T;
        }
        
        
        protected internal virtual T As<T>(ElaValue value) where T : ElaObject
        {
            return this as T;
        }


        protected internal virtual int AsInteger(ElaValue value)
        {
            throw InvalidCast(ElaTypeCode.Integer, value.GetTag());
        }


        protected internal virtual float AsSingle(ElaValue value)
        {
			throw InvalidCast(ElaTypeCode.Single, value.GetTag());
        }


        protected internal virtual bool AsBoolean(ElaValue value)
        {
			throw InvalidCast(ElaTypeCode.Boolean, value.GetTag());
        }


        protected internal virtual char AsChar(ElaValue value)
        {
			throw InvalidCast(ElaTypeCode.Char, value.GetTag());
        }


        protected InvalidCastException InvalidCast(ElaTypeCode target, string source)
        {
            return InvalidCast(TypeCodeFormat.GetShortForm(target), source);
        }


        protected InvalidCastException InvalidCast(string target, string source)
        {
            return new InvalidCastException(Strings.GetMessage("InvalidCast", source, target));
        }


		public virtual ElaTypeInfo GetTypeInfo()
		{
            var info = new ElaTypeInfo();
            info.AddField(TAG, GetTag());
            info.AddField(TYPEID, TypeId);
            info.AddField(ISBYREF, 
                TypeId == ElaMachine.INT ||
                TypeId == ElaMachine.CHR ||
                TypeId == ElaMachine.REA ||
                TypeId == ElaMachine.BYT);
            return info;
		}

		internal protected virtual int Compare(ElaValue @this, ElaValue other)
		{
			return @this.Ref == other.Ref ? 0 : -1;
		}


		public override string ToString()
		{
			return TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId);
		} 


		internal static ElaValue GetDefault()
		{
			return new ElaValue(ElaInvalidObject.Instance);
		}


        protected ElaValue Default()
        {
            return new ElaValue(ElaInvalidObject.Instance);
        }
		#endregion


		#region Properties
		public int TypeId { get; internal set; }
		#endregion
    }
}
