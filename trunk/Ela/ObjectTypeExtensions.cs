using System;

namespace Ela
{
	public static class ObjectTypeExtensions
	{
		#region Construction
		private const string ERR = "INVALID";
		private const string CHAR = "char";
		private const string INT = "int";
		private const string LONG = "long";
		private const string SINGLE = "single";
		private const string DOUBLE = "double";
		private const string STRING = "string";
		private const string BOOL = "bool";
		private const string RECORD = "record";
		private const string TUPLE = "tuple";
		private const string ARRAY = "array";
		private const string LIST = "list";
		private const string FUN = "function";
		private const string UNIT = "unit";
		private const string MOD = "module";
		private const string OBJ = "object";
		private const string LAZ = "lazy";
		private const string VAR = "variant";
		#endregion


		#region Methods
		public static string GetShortForm(this ObjectType @this)
		{
			switch (@this)
			{
				case ObjectType.Char: return CHAR;
				case ObjectType.Integer: return INT;
				case ObjectType.Long: return LONG;
				case ObjectType.Single: return SINGLE;
				case ObjectType.Double: return DOUBLE;
				case ObjectType.Boolean: return BOOL;
				case ObjectType.String: return STRING;
				case ObjectType.Array: return ARRAY;
				case ObjectType.List: return LIST;
				case ObjectType.Tuple: return TUPLE;
				case ObjectType.Record: return RECORD;
				case ObjectType.Function: return FUN;
				case ObjectType.Unit: return UNIT;
				case ObjectType.Module: return MOD;
				case ObjectType.Object: return OBJ;
				case ObjectType.Lazy: return LAZ;
				case ObjectType.Variant: return VAR;
				default: return ERR;
			}
		}
		#endregion
	}
}
