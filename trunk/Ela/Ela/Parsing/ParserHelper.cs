using System;
using System.Collections.Generic;
using System.Reflection;
using Ela.CodeModel;
using System.Globalization;

namespace Ela.Parsing
{
	internal sealed partial class Parser
	{
        private static readonly ElaEquation unit = new ElaEquation();
		private Stack<ElaEquation> bindings = new Stack<ElaEquation>(new ElaEquation[] { null });

        private void ProcessDoBlock(ElaExpression cexp1, ElaExpression cexp2, ref ElaJuxtaposition eqt)
        {
            if (eqt.Line == 0)
                eqt.SetLinePragma(cexp1.Line, cexp1.Column);

            if (eqt.Parameters.Count == 2 &&
                eqt.Parameters[0].Type == ElaNodeType.UnitLiteral && eqt.Parameters[0].Type == ElaNodeType.UnitLiteral)
            {
                eqt.Parameters.Clear();
                eqt.Target = null;
            }

            if (cexp2 != null)
            {
                if (eqt.Parameters.Count == 2)
                {
                    if (eqt.Parameters[1].Type == ElaNodeType.UnitLiteral)
                    {
                        var lambda3 = new ElaLambda();
                        lambda3.SetLinePragma(cexp2.Line, cexp2.Column);
                        lambda3.Left = cexp2;
                        var eqt2 = new ElaJuxtaposition();
                        eqt2.SetLinePragma(cexp2.Line, cexp2.Column);
                        lambda3.Right = eqt2;
                        eqt.Parameters[1] = lambda3;
                        eqt.Target = new ElaNameReference(t) { Name = ">>=" };
                        eqt = eqt2;
                    }
                    else
                    {
                        var oldeqt = eqt;
                        eqt = new ElaJuxtaposition();
                        eqt.Target = new ElaNameReference(t) { Name = ">>=" };
                        eqt.SetLinePragma(cexp1.Line, cexp1.Column);
                        eqt.Parameters.Add(oldeqt.Parameters[1]);
                        oldeqt.Parameters[1] = eqt;

                        var lambda2 = new ElaLambda();
                        lambda2.SetLinePragma(cexp2.Line, cexp2.Column);
                        lambda2.Left = cexp2;
                        var eqt2 = new ElaJuxtaposition();
                        eqt2.SetLinePragma(cexp2.Line, cexp2.Column);
                        eqt2.Target = new ElaNameReference(t) { Name = ">>-" };
                        eqt2.Parameters.Add(new ElaUnitLiteral(t));
                        eqt2.Parameters.Add(new ElaUnitLiteral(t));
                        lambda2.Right = eqt2;
                        eqt.Parameters.Add(lambda2);
                        eqt = eqt2;
                    }
                }
                else
                {
                    eqt.Target = new ElaNameReference(t) { Name = ">>=" };
                    eqt.Parameters.Add(cexp2);

                    var lambda = new ElaLambda();
                    lambda.SetLinePragma(cexp2.Line, cexp2.Column);
                    lambda.Left = cexp1;
                    var eqt2 = new ElaJuxtaposition();
                    eqt2.SetLinePragma(cexp2.Line, cexp2.Column);
                    eqt2.Target = new ElaNameReference(t) { Name = ">>-" };
                    eqt2.Parameters.Add(new ElaUnitLiteral(t));
                    eqt2.Parameters.Add(new ElaUnitLiteral(t));
                    lambda.Right = eqt2;

                    eqt.Parameters.Add(lambda);
                    eqt = eqt2;
                }
            }
            else
            {
                if (eqt.Parameters.Count == 2)
                {
                    if (eqt.Parameters[1].Type == ElaNodeType.UnitLiteral)
                        eqt.Parameters[1] = cexp1;
                    else
                    {
                        var oldeqt = eqt;
                        eqt = new ElaJuxtaposition();
                        eqt.Target = new ElaNameReference(t) { Name = ">>-" };
                        eqt.SetLinePragma(cexp1.Line, cexp1.Column);
                        eqt.Parameters.Add(oldeqt.Parameters[1]);
                        eqt.Parameters.Add(cexp1);
                        oldeqt.Parameters[1] = eqt;
                    }
                }
                else
                {
                    eqt.Target = new ElaNameReference(t) { Name = ">>-" };
                    eqt.Parameters.Add(cexp1);
                    eqt.Parameters.Add(new ElaUnitLiteral(t));
                }
            }
        }

		private bool RequireEndBlock()
		{
			if (la.kind == _EBLOCK)
				return true;

			if (la.kind == _PIPE)
				return false;

			if (la.kind == _IN)
			{
				scanner.PopIndent();
				return false;
			}

			return true;
		}

        private ElaVariableFlags ProcessAttribute(string attribute, ElaVariableFlags flags)
        {
            if (attribute == "private")
                return flags | ElaVariableFlags.Private;
            else if (attribute == "qualified")
                return flags | ElaVariableFlags.Qualified;
            else
            {
                AddError(ElaParserError.UnknownAttribute, attribute);
                return flags;
            }
        }

        private void BuildMask(ref int count, ref int mask, string val, string targ)
        {
            var flag = 0;

            if (val == targ)
                flag = 1;
            else if (val != "_")
                AddError(ElaParserError.InvalidFunctionSignature, val);

            mask |= flag << count;
            count++;
        }

        private void ProcessBinding(ElaEquationSet block, ElaEquation bid, ElaExpression left, ElaExpression right)
        {
            bid.Left = left;
            bid.Right = right;

            if (bindings.Peek() == unit)
            {
                block.Equations.Add(bid);
                return;
            }

            var fName = default(String);

            if (right != null && left.Type == ElaNodeType.Juxtaposition && !left.Parens)
            {
                var fc = (ElaJuxtaposition)left;

                if (fc.Target.Type == ElaNodeType.NameReference)
                    fName = fc.Target.GetName();
            }

            if (fName != null)
            {
                var lastB = bindings.Peek();

                if (lastB != null && ((ElaJuxtaposition)lastB.Left).Target.GetName() == fName)
                    lastB.Next = bid;
                else
                    block.Equations.Add(bid);

                bindings.Pop();
                bindings.Push(bid);
            }
            else
                block.Equations.Add(bid);
        }

		private ElaExpression GetOperatorFun(string op, ElaExpression left, ElaExpression right)
		{
            var fc = new ElaJuxtaposition(t) {
                Target = new ElaNameReference(t) { Name = op }
            };

            if (left != null)
                fc.Parameters.Add(left);
            else
                fc.FlipParameters = true;

            if (right != null)
                fc.Parameters.Add(right);

            return fc;
		}


		private ElaExpression GetPrefixFun(ElaExpression funexp, ElaExpression par, bool flip)
		{
			var fc = new ElaJuxtaposition(t) { Target = funexp };
			fc.Parameters.Add(par);
			fc.FlipParameters = flip;
			return fc;
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
				
				if (!Double.TryParse(val, NumberStyles.Float, Culture.NumberFormat, out res))
					AddError(ElaParserError.InvalidRealSyntax);
				
				return new ElaLiteralValue(res);
			}
            else
            {
                var res = default(Single);

                if (!Single.TryParse(val.Trim('f', 'F'), NumberStyles.Float, Culture.NumberFormat, out res))
                    AddError(ElaParserError.InvalidRealSyntax);

                return new ElaLiteralValue(res);
            }
		}


		private string ReadString(string val)
		{
			if (val.Length > 0) 
			{
				if (val[0] != '<')
				{
					var res = EscapeCodeParser.Parse(ref val);

					if (res > 0)
						AddError(ElaParserError.InvalidEscapeCode, res);
				}
				else
					val = val.Substring(2, val.Length - 4);
			}
			
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

		public ElaProgram Program { get; private set; }
	}
}
