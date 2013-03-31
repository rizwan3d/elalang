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

        private ElaExpression ValidateDoBlock(ElaExpression exp)
        {
            if (exp.Type == ElaNodeType.Juxtaposition && ((ElaJuxtaposition)exp).Parameters[0] == null)
                return ((ElaJuxtaposition)exp).Parameters[1];

            var root = exp;

            while (true)
            {
                if (exp.Type == ElaNodeType.Juxtaposition)
                {
                    var juxta = (ElaJuxtaposition)exp;

                    if (juxta.Parameters.Count == 2)
                    {
                        juxta.Parameters[0] = Reduce(juxta.Parameters[0], juxta);
                        juxta.Parameters[1] = Reduce(juxta.Parameters[1], juxta);
                        exp = juxta.Parameters[1];
                    }
                    else
                        break;
                }
                else if (exp.Type == ElaNodeType.Lambda)
                {
                    var lb = (ElaLambda)exp;
                    lb.Right = Reduce(lb.Right, lb);
                    exp = lb.Right;
                }
                else
                    break;
            }

            return root;
        }

        private ElaExpression Reduce(ElaExpression exp, ElaExpression parent)
        {
            if (exp == null)
            {
                errors.AddErr(parent.Line, parent.Column, ElaParserError.InvaliDoEnd);
                return new ElaUnitLiteral();
            }

            if (exp.Type == ElaNodeType.Juxtaposition)
            {
                var juxta = (ElaJuxtaposition)exp;

                if (juxta.Parameters[0] == null)
                    return juxta.Parameters[1];
                else
                    return juxta;
            }
            else
                return exp;
        }

        private void ProcessDoBlock(ElaExpression cexp1, ElaExpression cexp2, ref ElaExpression rootExp)
        {
            var eqt = default(ElaJuxtaposition);
            var lam = default(ElaLambda);
            var letb = default(ElaLetBinding);

            if (rootExp.Type == ElaNodeType.Juxtaposition)
                eqt = (ElaJuxtaposition)rootExp;
            else if (rootExp.Type == ElaNodeType.Lambda)
            {
                lam = (ElaLambda)rootExp;

                if (lam.Right != null && (lam.Right.Type != ElaNodeType.Juxtaposition || !((ElaJuxtaposition)lam.Right).Spec))
                {
                    eqt = new ElaJuxtaposition { Spec = true };
                    eqt.SetLinePragma(cexp1.Line, cexp1.Column);
                    eqt.Target = new ElaNameReference(t) { Name = ">>-" };
                    eqt.Parameters.Add(null);
                    eqt.Parameters.Add(lam.Right);
                    lam.Right = eqt;
                }
                else
                    eqt = lam.Right as ElaJuxtaposition;
            }
            else if (rootExp.Type == ElaNodeType.LetBinding)
            {
                letb = (ElaLetBinding)rootExp;

                if (letb.Expression != null && (letb.Expression.Type != ElaNodeType.Juxtaposition || ((ElaJuxtaposition)letb.Expression).Spec))
                {
                    eqt = new ElaJuxtaposition { Spec = true };
                    eqt.SetLinePragma(cexp1.Line, cexp1.Column);
                    eqt.Target = new ElaNameReference(t) { Name = ">>-" };
                    eqt.Parameters.Add(null);
                    eqt.Parameters.Add(letb.Expression);
                    letb.Expression = eqt;
                }
                else                    
                    eqt = letb.Expression as ElaJuxtaposition;
            }
            else if (rootExp.Type != ElaNodeType.None)
            {
                eqt = new ElaJuxtaposition { Spec = true };
                eqt.SetLinePragma(cexp1.Line, cexp1.Column);
                eqt.Parameters.Add(null);
                eqt.Parameters.Add(rootExp);
            }

            if (eqt != null && !eqt.Spec)
            {
                var eqt2 = new ElaJuxtaposition();
                eqt2.SetLinePragma(eqt.Line, eqt.Column);
                eqt2.Target = new ElaNameReference(t) { Name = ">>-" };
                eqt2.Parameters.Add(null);
                eqt2.Parameters.Add(eqt);
                eqt = eqt2;
            }

            if (cexp2 != null)
            {
                if (eqt != null && eqt.Parameters.Count == 2)
                {
                    if (eqt.Parameters[0] == null)
                    {
                        eqt.Target = new ElaNameReference(t) { Name = ">>=" };
                        
                        var lambda = new ElaLambda();
                        lambda.SetLinePragma(cexp2.Line, cexp2.Column);
                        lambda.Left = new ElaPlaceholder();
                        
                        var lambda2 = new ElaLambda();
                        lambda2.SetLinePragma(cexp2.Line, cexp2.Column);
                        lambda2.Left = cexp1;

                        var eqt1 = new ElaJuxtaposition { Spec = true };
                        eqt1.SetLinePragma(cexp2.Line, cexp2.Column);
                        eqt1.Target = new ElaNameReference(t) { Name = ">>=" };
                        eqt1.Parameters.Add(cexp2);
                        eqt1.Parameters.Add(lambda2);
                        lambda.Right = eqt1;

                        //var eqt2 = new ElaJuxtaposition();
                        //eqt2.SetLinePragma(cexp2.Line, cexp2.Column);
                        //eqt2.Target = new ElaNameReference(t) { Name = ">>-" };
                        //eqt2.Parameters.Add(null);
                        //eqt2.Parameters.Add(null);                        
                        //lambda2.Right = eqt2;

                        eqt.Parameters[0] = eqt.Parameters[1];
                        eqt.Parameters[1] = lambda;
                        //rootExp = eqt2;
                        rootExp = lambda2;
                    }
                    else
                    {
                        throw new Exception("Unable to process do-notation.");
                    }
                }
                else
                {
                    if (eqt == null)
                    {
                        eqt = new ElaJuxtaposition { Spec = true };
                        eqt.SetLinePragma(cexp1.Line, cexp1.Column);

                        if (lam != null)
                            lam.Right = eqt;
                        else if (letb != null)
                            letb.Expression = eqt;
                    }

                    eqt.Target = new ElaNameReference(t) { Name = ">>=" };
                    eqt.Parameters.Add(cexp2);

                    var lambda = new ElaLambda();
                    lambda.SetLinePragma(cexp2.Line, cexp2.Column);
                    lambda.Left = cexp1;
                    //var eqt2 = new ElaJuxtaposition();
                    //eqt2.SetLinePragma(cexp2.Line, cexp2.Column);
                    //eqt2.Target = new ElaNameReference(t) { Name = ">>-" };
                    //eqt2.Parameters.Add(null);
                    //eqt2.Parameters.Add(null);
                    //lambda.Right = eqt2;

                    eqt.Parameters.Add(lambda);
                    //rootExp = eqt2;
                    rootExp = lambda;
                }
            }
            else
            {
                if (eqt != null && eqt.Parameters.Count == 2)
                {
                    if (eqt.Parameters[0] == null)
                    {
                        eqt.Parameters[0] = eqt.Parameters[1];
                        eqt.Parameters[1] = cexp1;
                    }
                    else
                    {
                        var oldeqt = eqt;
                        eqt = new ElaJuxtaposition();
                        eqt.Target = new ElaNameReference(t) { Name = ">>-" };
                        eqt.SetLinePragma(cexp1.Line, cexp1.Column);
                        eqt.Parameters.Add(oldeqt.Parameters[1]);
                        eqt.Parameters.Add(cexp1);
                        oldeqt.Parameters[1] = eqt;
                        rootExp = eqt;
                    }
                }
                else if (eqt != null)
                {
                    eqt.Target = new ElaNameReference(t) { Name = ">>-" };
                    eqt.Parameters.Add(null);
                    eqt.Parameters.Add(cexp1);
                }
                else
                {
                    if (lam != null)
                        lam.Right = cexp1;
                    else if (letb != null)
                        letb.Expression = cexp1;
                    else
                        rootExp = cexp1;
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
