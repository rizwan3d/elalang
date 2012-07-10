using System;

namespace Ela
{
	public static class TCF
	{
		internal const string ERR = "INVALID";
		internal const string CHAR = "char";
		internal const string INT = "int";
		internal const string LONG = "long";
		internal const string SINGLE = "single";
		internal const string DOUBLE = "double";
		internal const string STRING = "string";
		internal const string BOOL = "bool";
		internal const string RECORD = "record";
		internal const string TUPLE = "tuple";
		internal const string LIST = "list";
		internal const string FUN = "fun";
		internal const string UNIT = "unit";
		internal const string MOD = "module";
		internal const string OBJ = "object";
		internal const string LAZ = "thunk";
		internal const string VAR = "variant";
        internal const string TYP = "typeinfo";
		
        public static ElaTypeCode GetTypeCode(string type)
        {
            switch (type)
            {
                case CHAR: return ElaTypeCode.Char;
                case INT: return ElaTypeCode.Integer;
                case LONG: return ElaTypeCode.Long;
                case SINGLE: return ElaTypeCode.Single;
                case DOUBLE: return ElaTypeCode.Double;
                case BOOL: return ElaTypeCode.Boolean;
                case STRING: return ElaTypeCode.String;
                case LIST: return ElaTypeCode.List;
                case TUPLE: return ElaTypeCode.Tuple;
                case RECORD: return ElaTypeCode.Record;
                case FUN: return ElaTypeCode.Function;
                case UNIT: return ElaTypeCode.Unit;
                case MOD: return ElaTypeCode.Module;
                case OBJ: return ElaTypeCode.Object;
                case LAZ: return ElaTypeCode.Lazy;
                case VAR: return ElaTypeCode.Variant;
                case TYP: return ElaTypeCode.TypeInfo;
                default: return ElaTypeCode.None;
            }
        }

		public static string GetShortForm(ElaTypeCode @this)
		{
			switch (@this)
			{
				case ElaTypeCode.Char: return CHAR;
				case ElaTypeCode.Integer: return INT;
				case ElaTypeCode.Long: return LONG;
				case ElaTypeCode.Single: return SINGLE;
				case ElaTypeCode.Double: return DOUBLE;
				case ElaTypeCode.Boolean: return BOOL;
				case ElaTypeCode.String: return STRING;
				case ElaTypeCode.List: return LIST;
				case ElaTypeCode.Tuple: return TUPLE;
				case ElaTypeCode.Record: return RECORD;
				case ElaTypeCode.Function: return FUN;
				case ElaTypeCode.Unit: return UNIT;
				case ElaTypeCode.Module: return MOD;
				case ElaTypeCode.Object: return OBJ;
				case ElaTypeCode.Lazy: return LAZ;
				case ElaTypeCode.Variant: return VAR;
                case ElaTypeCode.TypeInfo: return TYP;
				default: return ERR;
			}
		}
	}
}
