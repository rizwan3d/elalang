using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
    internal sealed partial class Builder
    {
        private void RewriteOrder(ElaBlock block, LabelMap map)
        {
            var list = ProcessSafeExpressions(block.Expressions, map);
            list = ProcessFunctions(list, map);
        }

        private FastList<ElaExpression> ProcessSafeExpressions(List<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaExpression>(len);

            for (var i = 0; i < len; i++)
            {
                var hints = i == len - 1 ? Hints.None : Hints.Left;
                var e = exps[i];

                if (e.Type == ElaNodeType.Binding)
                {
                    var b = (ElaBinding)e;

                    if (b.InitExpression != null)
                    {
                        if (b.InitExpression.Type != ElaNodeType.FunctionLiteral && b.InitExpression.Type != ElaNodeType.LazyLiteral
                            && b.Safe())
                        {
                            CompileDeclaration(b, map, hints);
                            continue;
                        }
                    }
                }

                list.Add(e);
            }

            return list;
        }

        private FastList<ElaExpression> ProcessFunctions(FastList<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaExpression>(len);

            for (var i = 0; i < len; i++)
            {
                var e = exps[i];
                var hints = i == len - 1 ? Hints.None : Hints.Left;
                
                if (e.Type == ElaNodeType.Binding)
                {
                    var b = (ElaBinding)e;

                    if (b.InitExpression != null)
                    {
                        if (b.InitExpression.Type == ElaNodeType.FunctionLiteral || b.InitExpression.Type == ElaNodeType.LazyLiteral)
                        {
                            CompileDeclaration(b, map, hints);
                            continue;
                        }
                    }
                }

                list.Add(e);
            }

            return list;
        }

        private FastList<ElaExpression> ProcessBindings(FastList<ElaExpression> exps, LabelMap map)
        {
            var len = exps.Count;
            var list = new FastList<ElaExpression>(len);

            for (var i = 0; i < len; i++)
            {
                var e = exps[i];
                var hints = i == len - 1 ? Hints.None : Hints.Left;

                if (e.Type == ElaNodeType.Binding)
                {
                    var b = (ElaBinding)e;

                    if (b.InitExpression == null)
                    {
                        CompileDeclaration(b, map, hints);
                        continue;
                    }

                    if (!TryStrict(b.InitExpression))
                        b.InitExpression.Flags |= ElaExpressionFlags.Lazy;
                    
                    CompileDeclaration(b, map, hints);
                    continue;
                }

                list.Add(e);
            }

            return list;
        }

        private bool TryStrict(ElaExpression exp)
        {
            if (exp == null)
                return true;

            switch (exp.Type)
            {
                case ElaNodeType.Builtin:
                    return true;
                case ElaNodeType.Generator:
                    {
                        var e = (ElaGenerator)exp;
                        return TryStrict(e.Body) && TryStrict(e.Guard) && TryStrict(e.Target);
                    }
                case ElaNodeType.Range:
                    {
                        var e = (ElaRange)exp;
                        return TryStrict(e.First) && TryStrict(e.Last) && TryStrict(e.Second);
                    }
                case ElaNodeType.VariantLiteral:
                    return true;
                case ElaNodeType.LazyLiteral:
                    return true;
                case ElaNodeType.Try:
                    {
                        var e = (ElaTry)exp;

                        if (!TryStrict(e.Expression))
                            return false;

                        foreach (var en in e.Entries)
                            if (!TryStrict(en))
                                return false;

                        return true;
                    }
                case ElaNodeType.Comprehension:
                    {
                        var e = (ElaComprehension)exp;
                        return TryStrict(e.Generator);
                    }
                case ElaNodeType.Match:
                    {
                        var e = (ElaMatch)exp;

                        if (!TryStrict(e.Expression))
                            return false;

                        foreach (var en in e.Entries)
                            if (!TryStrict(en))
                                return false;

                        return true;
                    }
                case ElaNodeType.MatchEntry:
                    {
                        var e = (ElaMatchEntry)exp;
                        return TryStrict(e.Expression) && TryStrict(e.Guard);
                    }
                case ElaNodeType.FunctionLiteral:
                    return true;
                case ElaNodeType.Binding:
                    {
                        var e = (ElaBinding)exp;
                        return e.Pattern == null && TryStrict(e.InitExpression);
                    }
                case ElaNodeType.Condition:
                    {
                        var e = (ElaCondition)exp;
                        return TryStrict(e.Condition) && TryStrict(e.True) && TryStrict(e.False);
                    }
                case ElaNodeType.Raise:
                    {
                        var e = (ElaRaise)exp;
                        return TryStrict(e.Expression);
                    }
                case ElaNodeType.Binary:
                    {
                        var e = (ElaBinary)exp;
                        return TryStrict(e.Left) && TryStrict(e.Right);
                    }
                case ElaNodeType.Primitive:
                    return true;
                case ElaNodeType.RecordLiteral:
                    {
                        var e = (ElaRecordLiteral)exp;

                        foreach (var f in e.Fields)
                            if (!TryStrict(f.FieldValue))
                                return false;

                        return true;
                    }
                case ElaNodeType.FieldReference:
                    {
                        var e = (ElaFieldReference)exp;
                        return TryStrict(e.TargetObject);
                    }
                case ElaNodeType.ListLiteral:
                    {
                        var e = (ElaListLiteral)exp;
                        
                        if (!e.HasValues())
                            return true;

                        foreach (var v in e.Values)
                            if (!TryStrict(v))
                                return false;

                        return true;
                    }
                case ElaNodeType.VariableReference:
                    {
                        var v = (ElaVariableReference)exp;
                        return (GetVariable(v.VariableName, CurrentScope, GetFlags.NoError, 0, 0).Flags & ElaVariableFlags.NoInit) != ElaVariableFlags.NoInit;
                    }
                case ElaNodeType.Placeholder:
                    return true;
                case ElaNodeType.FunctionCall:
                    {
                        var v = (ElaFunctionCall)exp;

                        if (v.Target.Type != ElaNodeType.VariableReference)
                            return false;

                        var tv = GetVariable(v.Target.GetName(), CurrentScope, GetFlags.NoError, 0, 0);
                        return (tv.Flags & ElaVariableFlags.Function) == ElaVariableFlags.Function &&
                            tv.Data > v.Parameters.Count;
                    }
                case ElaNodeType.Is:
                    {
                        var v = (ElaIs)exp;
                        return TryStrict(v.Expression);
                    }
                case ElaNodeType.UnitLiteral:
                    return true;
                case ElaNodeType.TupleLiteral:
                    {
                        var v = (ElaTupleLiteral)exp;

                        if (!v.HasParameters)
                            return true;

                        foreach (var e in v.Parameters)
                            if (!TryStrict(e))
                                return false;

                        return true;
                    }
                default:
                    return false;
            }
        }

        
        private void RewritePattern(ElaBinding b, LabelMap map, Hints hints)
        {

        }
    }
}
