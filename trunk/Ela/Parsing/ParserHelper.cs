﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Ela.CodeModel;
using System.Globalization;

namespace Ela.Parsing
{
	internal sealed partial class Parser
	{
		private static readonly ElaVariableReference hiddenVar = new ElaVariableReference { VariableName = "$0" };
		private static readonly ElaVariablePattern hiddenPattern = new ElaVariablePattern { Name = "$0" };

		#region Methods
		private ElaExpression GetBuiltin(Token t, string value)
		{
			var kind = Builtins.Kind(value);
			return kind != ElaBuiltinFunctionKind.None ? (ElaExpression)new ElaBuiltinFunction(t) { Kind = kind } :
				(ElaExpression)new ElaVariableReference(t) { VariableName = value };
		}


		//private bool RequireEnd(ElaExpression exp)
		//{
		//    if (la.kind == _ENDS)
		//        return true;

		//    if (exp.Type == ElaNodeType.Binding)
		//    {
		//        var vexp = (ElaBinding)exp;
				
		//        if (vexp.And != null)
		//            return true;

		//        if (vexp.InitExpression.Type != ElaNodeType.FunctionLiteral)
		//            return false;
				
		//        return ((ElaFunctionLiteral)vexp.InitExpression).Body.Entries.Count > 1;
		//    }
		//    else
		//        return true;
		//}


		private bool RequireSemicolon()
		{
			return la.kind != _EOF && la.kind != _LET && la.kind != _OPEN && t.kind != _SEMI && t.kind != _ENDS;
		}


		private void SetFunMetadata(ElaBinding varExp, ElaExpression cexp, ElaVariableFlags flags)
		{
			if (cexp != null)
			{
				var imm = (flags & ElaVariableFlags.Immutable) == ElaVariableFlags.Immutable;

				if (cexp.Type == ElaNodeType.FunctionLiteral && imm && varExp.VariableName != null)
				{
					((ElaFunctionLiteral)cexp).Name = varExp.VariableName;
					varExp.VariableFlags |= ElaVariableFlags.Function;
				}
				else if (cexp.Type < ElaNodeType.FunctionLiteral && imm && varExp.VariableName != null)
					varExp.VariableFlags |= ElaVariableFlags.ObjectLiteral;
			}
		}


		private ElaExpression GetOperatorFun(ElaExpression exp, ElaOperator op, bool right)
		{
            if (la.kind == _MINUS)
                AddError(ElaParserError.UnexpectedMinus);

			var bin = new ElaBinary(t) 
			{ 
				Operator = op,
				Left = !right ? exp : hiddenVar,
				Right = right ? exp : hiddenVar
			};
			var m = new ElaMatch();
			m.SetLinePragma(exp.Line, exp.Column);
			m.Entries.Add(new ElaMatchEntry { Expression = bin, Pattern = hiddenPattern });

			var ret = new ElaFunctionLiteral { Body = m };
			ret.SetLinePragma(exp.Line, exp.Column);
			return ret;
		}


		private ElaExpression GetCustomOperatorFun(string name, ElaExpression par)
		{
			return new ElaBinary(t)
			{
				Operator = ElaOperator.Custom,
				CustomOperator = name,
				Right = par
			};
		}


		private ElaExpression GetPrefixFun(string name, ElaExpression par, bool flip)
		{
			var fc = new ElaFunctionCall(t)
			{
				Target = new ElaVariableReference(t) { VariableName = name }
			};

			fc.Parameters.Add(par);
			fc.FlipParameters = flip;
			return fc;
		}


		private ElaExpression GetPartialFun(ElaExpression exp)
		{
			var m = new ElaMatch { Expression = hiddenVar };
			m.SetLinePragma(exp.Line, exp.Column);
			m.Entries.Add(new ElaMatchEntry { Expression = exp, Pattern = hiddenPattern });			
			var ret = new ElaFunctionLiteral() { Body = m };
			ret.SetLinePragma(exp.Line, exp.Column);
			return ret;
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
				val == "record" ? ObjectType.Record :
				val == "tuple" ? ObjectType.Tuple :
				val == "fun" ? ObjectType.Function :
				val == "object" ? ObjectType.Object :
				val == "unit" ? ObjectType.Unit :
				val == "module" ? ObjectType.Module :
				val == "lazy" ? ObjectType.Lazy :
				val == "variant" ? ObjectType.Variant :
				TypeError(val);
		}


		private ObjectType TypeError(string val)
		{
			AddError(ElaParserError.UnknownConversionType, val);
			return ObjectType.None;
		}


		private ElaLiteralValue ParseInt(string val)
		{
			return ParseInt(val, false);
		}


		private ElaLiteralValue ParseInt(string val, bool negate)
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
				
				return new ElaLiteralValue(negate ? -res : res);
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
				
				return new ElaLiteralValue(negate ? -res : res);
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
			return ParseReal(val, false);
		}


		private ElaLiteralValue ParseReal(string val, bool negate)
		{
			if (TrimLast(ref val, 'd', 'D'))
			{
				var res = default(Double);
				
				if (!Double.TryParse(val, NumberStyles.Float, Culture.NumberFormat, out res))
					AddError(ElaParserError.InvalidRealSyntax);
				
				return new ElaLiteralValue(negate ? -res : res);
			}
			else
			{
				var res = default(Single);
				
				if (!Single.TryParse(val, NumberStyles.Float, Culture.NumberFormat, out res))
					AddError(ElaParserError.InvalidRealSyntax);
				
				return new ElaLiteralValue(negate ? -res : res);
			}
		}


		private string ReadString(string val)
		{
			if (val.Length > 0 && val[0] != '@')
			{
				var res = EscapeCodeParser.Parse(ref val);

				if (res > 0)
					AddError(ElaParserError.InvalidEscapeCode, res);
			}
			else
				val = val.Substring(2, val.Length - 3);
			
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
		public ElaExpression Expression { get; private set; }
		#endregion
	}
}
