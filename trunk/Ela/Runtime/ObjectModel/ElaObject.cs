using System;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public abstract class ElaObject
	{
		#region Construction
		internal static readonly ExecutionContext DummyContext = new ExecutionContext();
		internal const string UNDEF = "<unknown>";
        internal const string INVALID = "<INVALID>";
        private const string TYPENAME = "typeName";
        private const string TYPECODE = "typeCode";
        private const string ISBYREF = "byRef";

		protected ElaObject(TypeId typeId)
		{
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

            protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
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


        internal virtual string GetTag()
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
            throw InvalidCast(ElaTypeCode.Integer, value.GetTypeName());
        }


        protected internal virtual float AsSingle(ElaValue value)
        {
            throw InvalidCast(ElaTypeCode.Single, value.GetTypeName());
        }


        protected internal virtual bool AsBoolean(ElaValue value)
        {
            throw InvalidCast(ElaTypeCode.Boolean, value.GetTypeName());
        }


        protected internal virtual char AsChar(ElaValue value)
        {
            throw InvalidCast(ElaTypeCode.Char, value.GetTypeName());
        }


        protected InvalidCastException InvalidCast(ElaTypeCode target, string source)
        {
            return InvalidCast(TypeCodeFormat.GetShortForm(target), source);
        }


        protected InvalidCastException InvalidCast(string target, string source)
        {
            return new InvalidCastException(Strings.GetMessage("InvalidCast", source, target));
        }


		public virtual ElaPatterns GetSupportedPatterns()
		{
			return ElaPatterns.None;
		}


        public virtual ElaTypeInfo GetTypeInfo()
		{
            var info = new ElaTypeInfo();
            info.AddField(TYPENAME, GetTypeName());
            info.AddField(TYPECODE, TypeId);
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
			if (TypeId == ElaMachine.INT ||
				TypeId == ElaMachine.REA ||
				TypeId == ElaMachine.CHR ||
				TypeId == ElaMachine.BYT)
				return UNDEF;
			else
                return Show(new ElaValue(this), ShowInfo.Debug, DummyContext);
		}


		internal static ElaValue GetDefault()
		{
			return new ElaValue(ElaInvalidObject.Instance);
		}


        protected ElaValue Default()
        {
            return new ElaValue(ElaInvalidObject.Instance);
        }


		protected internal virtual string GetTypeName()
		{
			return TypeCodeFormat.GetShortForm((ElaTypeCode)TypeId);
		}


		private string ThisToString(ElaValue first, ElaValue second, ExecutionContext ctx)
		{
			return first.Ref == this ? first.Ref.Show(first, ShowInfo.Default, ctx) :
				second.Ref.Show(second, ShowInfo.Default, ctx);
		}


		private ElaValue This(ElaValue first, ElaValue second)
		{
			return first.Ref == this ? first : second;
		}
		#endregion


		#region Operations
		internal virtual bool Call(EvalStack stack, ExecutionContext ctx)
		{
			return false;
		}


		protected internal virtual bool Bool(ElaValue @this, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "bool");
			return false;
		}


		protected internal virtual ElaValue Generate(ElaValue value, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "gen");
			return Default();
		}


		protected internal virtual ElaValue GenerateFinalize(ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "genfin");
			return Default();
		}


		protected internal virtual ElaValue GetField(string field, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "fieldget");
			return Default();
		}


		protected internal virtual void SetField(string field, ElaValue value, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "fieldset");						
		}


		protected internal virtual bool HasField(string field, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "fieldhas");
			return false;
		}


		protected internal virtual string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			ctx.NoOperator(@this, "show");
			return String.Empty;
		}


		protected internal virtual ElaValue Call(ElaValue arg, ExecutionContext ctx)
		{
			ctx.NoOperator(new ElaValue(this), "call");
			return Default();
		}


		protected internal virtual ElaValue Force(ElaValue @this, ExecutionContext ctx)
		{
			return @this;
		}


		internal virtual ElaValue InternalForce(ElaValue @this, ExecutionContext ctx)
		{
			return @this;
		}


		protected internal virtual ElaValue Untag(ElaValue @this, ExecutionContext ctx)
        {
            return @this;
        }
		#endregion


		#region Properties
		internal int TypeId { get; set; }
		#endregion
	}
}
