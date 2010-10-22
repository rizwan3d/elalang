using System;
using System.Collections.Generic;
using System.Reflection;
using Ela.CodeModel;
using System.Globalization;

namespace Ela.Parsing
{
	internal sealed partial class Parser
	{
		private static CultureInfo culture = CultureInfo.GetCultureInfo("en-US");
		private FastStack<ElaFunctionLiteral> funs = new FastStack<ElaFunctionLiteral>();
		
		#region Methods
		private void InitializeCodeUnit()
		{
			CodeUnit = new ElaCodeUnit();
		}

			   
		private ElaExpression GetFun(ElaExpression exp)
		{
			if (exp != null && exp.Placeholders > 0 && exp.Type != ElaNodeType.Placeholder)
			{
				return new ElaFunctionLiteral(t)
				{
					Expression = exp,
					Currying = exp.Placeholders
				};
			}
			else
				return exp;
		}


		private ObjectType GetType(string val)
		{
			return
				val == "int" ? ObjectType.Integer :
				val == "single" ? ObjectType.Single :
				val == "bool" ? ObjectType.Boolean :
				val == "char" ? ObjectType.Char :
				val == "long" ? ObjectType.Long :
				val == "double" ? ObjectType.Double :
				val == "string" ? ObjectType.String :
				val == "list" ? ObjectType.List :
				val == "array" ? ObjectType.Array :
				val == "record" ? ObjectType.Record :
				val == "tuple" ? ObjectType.Tuple :
				val == "function" ? ObjectType.Function :
				val == "object" ? ObjectType.Object :
				val == "unit" ? ObjectType.Unit :
				val == "module" ? ObjectType.Module :
				val == "seq" ? ObjectType.Sequence :
				TypeError(val);
		}


		private ObjectType TypeError(string val)
		{
			AddError(ElaParserError.UnknownConversionType, val);
			return ObjectType.None;
		}


		private ElaLiteralValue ParseInt(string val)
		{
			if (TrimLast(ref val, 'l', 'L'))
			{
				var res = default(Int64);
				
				if (!Int64.TryParse(val, out res))
				{
					try
					{
						res = Convert.ToInt64(val, 16);
					}
					catch 
					{
						AddError(ElaParserError.InvalidIntegerSyntax);
					}
				}
				
				return new ElaLiteralValue(res);
			}
			else
			{
				var res = default(Int32);
				
				if (!Int32.TryParse(val, out res))
				{
					try
					{
						res = Convert.ToInt32(val, 16);
					}
					catch 
					{
						AddError(ElaParserError.InvalidIntegerSyntax);
					}
				}
				
				return new ElaLiteralValue(res);
			}
		}


		private ElaLiteralValue ParseString(string val)
		{
			return new ElaLiteralValue(ReadString(val));
		}
		
		
		private ElaLiteralValue ParseChar(string val)
		{
			var str = ReadString(val);
			return new ElaLiteralValue(str[0]);
		}


		private ElaLiteralValue ParseReal(string val)
		{
			if (TrimLast(ref val, 'd', 'D'))
			{
				var res = default(Double);
				
				if (!Double.TryParse(val, NumberStyles.Float, culture.NumberFormat, out res))
					AddError(ElaParserError.InvalidRealSyntax);
				
				return new ElaLiteralValue(res);
			}
			else
			{
				var res = default(Single);
				
				if (!Single.TryParse(val, NumberStyles.Float, culture.NumberFormat, out res))
					AddError(ElaParserError.InvalidRealSyntax);
				
				return new ElaLiteralValue(res);
			}
		}


		private string ReadString(string val)
		{
			var res = EscapeCodeParser.Parse(ref val);
			
			if (res > 0)
				AddError(ElaParserError.InvalidEscapeCode, res);
			
			return val;
		}
		
		
		private bool TrimLast(ref string val, char cl, char cu)
		{			
			var lc = val[val.Length - 1];			
			
			if (lc == cl || lc == cu)
			{
				val = val.Remove(val.Length - 1, 1);
				return true;
			}
			else
				return false;
		}
		#endregion


		#region Properties
		public ElaCodeUnit CodeUnit { get; private set; }
		#endregion
	}
}
