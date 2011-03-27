//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using Ela.CodeModel;
//using Ela.Parsing;
//using Ela.Runtime;
//using Ela.Runtime.ObjectModel;

//namespace Ela.Inference
//{
//    public sealed class TypeChecker
//    {
//        public void Run(string fileName)
//        {
//            var p = new ElaParser();
//            var ast = p.Parse(new FileInfo(fileName));
//            var scope = new BindingScope(null);
//            GetExpressionType(ast.Expression, scope);
//            PrintOutScope("", scope);
//        }


//        private void PrintOutScope(string pad, BindingScope scope)
//        {
//            foreach (var kv in scope.LocalBindings)
//                Console.WriteLine(pad + kv.Value.ToString());
//        }


//        private TypeTag GetBindingType(ElaBinding bin, BindingScope scope)
//        {
//            var parent = scope;

//            if (!(bin.Where == null && bin.In == null && bin.InitExpression.Type != ElaNodeType.FunctionLiteral))
//            {
//                var c = new BindingScope(scope);
//                scope.ChildScopes.Add(c);
//                scope = c;
//            }

//            if (bin.Where != null)
//                GetBindingType(bin.Where, scope);

//            if (bin.VariableName != null)
//            {
//                var type = default(TypeTag);				
//                var tt = GetExpressionType(bin.InitExpression, scope);

//                if (!tt.Complete)
//                    type = new TypeTag(tt.Traits, false);
//                else
//                    type = tt;

//                parent.LocalBindings.Add(bin.VariableName, new Binding(bin.VariableName, type));
//            }
//            else
//                GetPatternType(bin.Pattern, parent);

//            GetExpressionType(bin.InitExpression, scope);
			
//            if (bin.In != null)
//                return GetExpressionType(bin.In, scope);
//            else
//                return TypeTag.Unit();
//        }


//        private ElaTraits GetBinaryType(ElaBinary bin, BindingScope scope)
//        {
//            switch (bin.Operator)
//            {
//                case ElaOperator.Add: 
//                case ElaOperator.Subtract:
//                case ElaOperator.Multiply:
//                case ElaOperator.Remainder:
//                case ElaOperator.Divide:
//                case ElaOperator.Power:
//                    return ElaTraits.Num;
//                case ElaOperator.BitwiseAnd:
//                case ElaOperator.BitwiseOr:
//                case ElaOperator.BitwiseXor:
//                case ElaOperator.ShiftLeft:
//                case ElaOperator.ShiftRight:
//                    return ElaTraits.Bit;
//                case ElaOperator.BooleanAnd:
//                case ElaOperator.BooleanOr:
//                    return ElaTraits.Bool;
//                case ElaOperator.CompBackward:
//                case ElaOperator.CompForward:
//                    return ElaTraits.Call;
//                case ElaOperator.Concat:
//                    return ElaTraits.Concat;
//                case ElaOperator.Cons:
//                    return ElaTraits.Cons;
//                case ElaOperator.Lesser:
//                case ElaOperator.Greater:
//                case ElaOperator.LesserEqual:
//                case ElaOperator.GreaterEqual:
//                    return ElaTraits.Ord;
//                case ElaOperator.Equals:
//                case ElaOperator.NotEquals:
//                    return ElaTraits.Eq;
//                default:
//                    return ElaTraits.None;
//            }
//        }


//        private TypeTag GetExpressionType(ElaExpression exp, BindingScope scope)
//        {
//            switch (exp.Type)
//            {
//                case ElaNodeType.Argument: return TypeTag.Any();				
//                case ElaNodeType.Binding: return GetBindingType((ElaBinding)exp, scope);
//                case ElaNodeType.Binary:
//                    {
//                        var bin = (ElaBinary)exp;
//                        var type1 = GetExpressionType(bin.Left, scope);
//                        var type2 = GetExpressionType(bin.Right, scope);
//                        var binType = GetBinaryType(bin, scope);
//                        var ret = type1.Traits == type2.Traits && (type1.Traits & binType) == binType ? type1 :
//                            new TypeTag(type1.Traits | type2.Traits | binType, false);

//                        if (!type1.Complete && bin.Operator != ElaOperator.Cons)
//                            type1.Traits |= binType;

//                        if (!type2.Complete)
//                            type2.Traits |= binType;

//                        return ret;
//                    }
//                case ElaNodeType.Block:
//                    {
//                        var bl = (ElaBlock)exp;
//                        var type = TypeTag.Any();

//                        foreach (var e in bl.Expressions)
//                            type = GetExpressionType(e, scope);

//                        return type;
//                    }
//                case ElaNodeType.BuiltinFunction:
//                    {
//                        var bf = (ElaBuiltinFunction)exp;
//                        return GetBuiltinFunctionType(bf);
//                    }
//                case ElaNodeType.Cast:
//                    {
//                        var cst = (ElaCast)exp;
//                        var type = GetExpressionType(cst.Expression, scope);
//                        return (type.Traits & ElaTraits.Convert) == ElaTraits.Convert ? type :
//                            new TypeTag(type.Traits | ElaTraits.Convert, false);
//                    }
//                case ElaNodeType.Comprehension:
//                    {
//                        var ch = (ElaComprehension)exp;
//                        return GetExpressionType(ch.Initial, scope);
//                    }
//                case ElaNodeType.Condition:
//                    {
//                        var @if = (ElaCondition)exp;
//                        var type1 = GetExpressionType(@if.True, scope);
//                        var type2 = GetExpressionType(@if.False, scope);
//                        return type1.Traits != type2.Traits ? new TypeTag(type1.Traits | type2.Traits, false) : type1;
//                    }
//                case ElaNodeType.CustomOperator: return null; //TODO
//                case ElaNodeType.FieldReference:
//                    {
//                        var fr = (ElaFieldReference)exp;
//                        var type = GetExpressionType(fr.TargetObject, scope);

//                        if (type.Fields == null)
//                            return TypeTag.Any();
//                        else
//                        {
//                            for (var i = 0; i < type.Fields.Count; i++)
//                            {
//                                if (type.Fields[i].Name == fr.FieldName)
//                                    return type.Fields[i].Type;
//                            }

//                            return TypeTag.Any();
//                        }
//                    }
//                case ElaNodeType.FunctionCall:
//                    {
//                        var call = (ElaFunctionCall)exp;
//                        var type = GetExpressionType(call.Target, scope);

//                        if (!type.Complete)
//                            type.Traits |= ElaTraits.Call;

//                        if (type.FunctionType == null)
//                            return TypeTag.Any(false);
//                        else
//                            return type.FunctionType[type.FunctionType.Count - 1];
//                    }
//                case ElaNodeType.FunctionLiteral: return GetFunctionType((ElaFunctionLiteral)exp, scope);
//                case ElaNodeType.Indexer: return TypeTag.Any();
//                case ElaNodeType.Is:
//                    {
//                        var eis = (ElaIs)exp;
//                        GetPatternType(eis.Pattern, scope);
//                        return new TypeTag(GetTraits(ElaTypeCode.Boolean), true);
//                    }
//                case ElaNodeType.LazyLiteral: return new TypeTag(ElaTraits.Thunk, false);
//                case ElaNodeType.ListLiteral: return new TypeTag(GetTraits(ElaTypeCode.List), true);
//                case ElaNodeType.Match:
//                    {
//                        var mt = (ElaMatch)exp;
//                        return GetMatchType(mt, scope);
//                    }
//                case ElaNodeType.MatchEntry:
//                    {
//                        var me = (ElaMatchEntry)exp;
//                        return GetExpressionType(me.Expression, scope);
//                    }
//                case ElaNodeType.ModuleInclude: return TypeTag.Unit();
//                case ElaNodeType.Primitive:
//                    return new TypeTag(GetTraits(((ElaPrimitive)exp).Value.LiteralType), true);
//                case ElaNodeType.Raise: return new TypeTag(ElaTraits.None, false);
//                case ElaNodeType.Range:
//                    {
//                        var rng = (ElaRange)exp;
//                        return GetExpressionType(rng.Initial, scope);
//                    }
//                case ElaNodeType.RecordLiteral:
//                    {
//                        var ret = new TypeTag(GetTraits(ElaTypeCode.Record), true);
//                        ret.Fields = new List<Binding>();
//                        var rec = (ElaRecordLiteral)exp;

//                        foreach (var f in rec.Fields)
//                        {
//                            var ft = f.Mutable ? new TypeTag(ElaTraits.None, true) : GetExpressionType(f.FieldValue, scope);
//                            ret.Fields.Add(new Binding(f.FieldName, ft));
//                        }

//                        return ret;
//                    }
//                case ElaNodeType.Try:
//                    {
//                        var @try = (ElaTry)exp;
//                        var type1 = GetExpressionType(@try.Expression, scope);
//                        var type2 = GetMatchType(@try, scope);
//                        return type1.Traits != type2.Traits ? new TypeTag(type1.Traits | type2.Traits, false) : type1;
//                    }
//                case ElaNodeType.TupleLiteral: 
//                    return new TypeTag(GetTraits(ElaTypeCode.Tuple), true);
//                case ElaNodeType.UnitLiteral: return TypeTag.Unit();
//                case ElaNodeType.VariableReference:
//                    {
//                        var v = (ElaVariableReference)exp;
//                        var bin = default(Binding);

//                        var s = scope;

//                        while (s != null)
//                        {
//                            if (s.LocalBindings.TryGetValue(v.VariableName, out bin))
//                                break;

//                            s = s.ParentScope;
//                        }

//                        return bin != null ? bin.Type : TypeTag.Any(false);
//                    }
//                case ElaNodeType.VariantLiteral:
//                    {
//                        var v = (ElaVariantLiteral)exp;
//                        var type = v.Expression != null ? GetExpressionType(v.Expression, scope) : TypeTag.Unit();

//                        var ret = new TypeTag(type.Traits | ElaTraits.Tag, true);
//                        ret.Fields = type.Fields;
//                        ret.FunctionType = type.FunctionType;
//                        return ret;
//                    }
//                default: return TypeTag.Any();
//            }
//        }


//        private ElaTraits GetTraits(ElaTypeCode type)
//        {
//            switch (type)
//            {
//                case ElaTypeCode.Boolean:
//                    return new ElaValue(true).Traits;
//                case ElaTypeCode.Char:
//                    return new ElaValue('\0').Traits;
//                case ElaTypeCode.Double:
//                    return new ElaValue(.0D).Traits;
//                case ElaTypeCode.Function:
//                    return new ElaValue(ElaFunction.Create<Int32>(() => 0)).Traits;
//                case ElaTypeCode.Integer:
//                    return new ElaValue(0).Traits;
//                case ElaTypeCode.Lazy:
//                    return ElaTraits.Thunk;
//                case ElaTypeCode.List:
//                    return new ElaValue(new ElaList(null, null)).Traits;
//                case ElaTypeCode.Long:
//                    return new ElaValue(0L).Traits;
//                case ElaTypeCode.Module:
//                    return ElaTraits.Eq | ElaTraits.Show;
//                case ElaTypeCode.Record:
//                    return new ElaValue(new ElaRecord(default(ElaRecordField))).Traits;
//                case ElaTypeCode.Single:
//                    return new ElaValue(.0).Traits;
//                case ElaTypeCode.String:
//                    return new ElaValue("").Traits;
//                case ElaTypeCode.Tuple:
//                    return new ElaValue(new ElaTuple(default(ElaValue))).Traits;
//                case ElaTypeCode.Unit:
//                    return ElaTraits.Eq | ElaTraits.Show;
//                case ElaTypeCode.Variant:
//                    return ElaTraits.Tag;
//                default:
//                    return ElaTraits.None;
//            }
//        }


//        private TypeTag GetBuiltinFunctionType(ElaBuiltinFunction bf)
//        {
//            var funType = new TypeTag(ElaTraits.Call, true);
//            funType.FunctionType = new List<TypeTag>();

//            switch (bf.Kind)
//            {
//                case ElaBuiltinFunctionKind.Bitnot:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Bit, false));
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Bit, false));
//                    break;
//                case ElaBuiltinFunctionKind.Flip:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Call, false));
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Call, false));
//                    break;
//                case ElaBuiltinFunctionKind.Force:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Thunk, false));
//                    funType.FunctionType.Add(TypeTag.Any());
//                    break;
//                case ElaBuiltinFunctionKind.Length:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Len, false));
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Num, true));
//                    break;
//                case ElaBuiltinFunctionKind.Max:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Bound, false));
//                    funType.FunctionType.Add(TypeTag.Any());
//                    break;
//                case ElaBuiltinFunctionKind.Min:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Bound, false));
//                    funType.FunctionType.Add(TypeTag.Any());
//                    break;
//                case ElaBuiltinFunctionKind.Negate:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Neg, false));
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Neg, true));
//                    break;
//                case ElaBuiltinFunctionKind.Not:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Bool, false));
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Bool, true));
//                    break;
//                case ElaBuiltinFunctionKind.Pred:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Enum, false));
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Enum, true));
//                    break;
//                case ElaBuiltinFunctionKind.Show:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Show, false));
//                    funType.FunctionType.Add(new TypeTag(GetTraits(ElaTypeCode.String), true));
//                    break;
//                case ElaBuiltinFunctionKind.Showf:
//                    funType.FunctionType.Add(new TypeTag(GetTraits(ElaTypeCode.String), true));
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Show, false));
//                    funType.FunctionType.Add(new TypeTag(GetTraits(ElaTypeCode.String), true));
//                    break;
//                case ElaBuiltinFunctionKind.Succ:
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Enum, false));
//                    funType.FunctionType.Add(new TypeTag(ElaTraits.Enum, true));
//                    break;
//                case ElaBuiltinFunctionKind.Type:
//                    funType.FunctionType.Add(TypeTag.Any());
//                    funType.FunctionType.Add(TypeTag.Any());
//                    break;
//                case ElaBuiltinFunctionKind.Typeid:
//                    funType.FunctionType.Add(TypeTag.Any());
//                    funType.FunctionType.Add(TypeTag.Any());
//                    break;
//                case ElaBuiltinFunctionKind.IsRef:
//                    funType.FunctionType.Add(TypeTag.Any());
//                    funType.FunctionType.Add(TypeTag.Any());
//                    funType.FunctionType.Add(TypeTag.Any());
//                    break;
//            }

//            return funType;
//        }


//        private TypeTag GetMatchType(ElaMatch mt, BindingScope scope)
//        {
//            var type = TypeTag.Any();
//            var lc = default(BindingScope);

//            foreach (var e in mt.Entries)
//            {
//                if (e.Pattern != null)
//                {
//                    lc = new BindingScope(scope);
//                    scope.ChildScopes.Add(lc);
//                    GetExpressionType(e.Pattern, lc);
//                }

//                var et = GetExpressionType(e, lc);

//                if (et.Traits != type.Traits)
//                    type = new TypeTag(et.Traits | type.Traits, false);

//                if (e.Where != null)
//                    GetBindingType((ElaBinding)e.Where, lc);
//            }

//            return type;
//        }


//        private TypeTag GetFunctionType(ElaFunctionLiteral fun, BindingScope scope)
//        {
//            var pars = new List<TypeTag>();
//            var bodyType = default(TypeTag);
//            var lc = default(BindingScope);
			
//            for (var i = 0; i < fun.ParameterCount; i++)
//                pars.Add(new TypeTag(ElaTraits.None, false));

//            foreach (var e in fun.Body.Entries)
//            {
//                if (e.Pattern != null)
//                {
//                    lc = new BindingScope(scope);
//                    scope.ChildScopes.Add(lc);

//                    if (e.Pattern.Type == ElaNodeType.PatternGroup)
//                    {
//                        var c = 0;

//                        foreach (var p in ((ElaPatternGroup)e.Pattern).Patterns)
//                        {
//                            var tt = GetPatternType(p, lc);

//                            if (pars[c] == null)
//                                pars[c] = tt;
//                            else
//                            {
//                                tt.Traits |= pars[c].Traits;

//                                if (tt.Fields == null)
//                                    tt.Fields = pars[c].Fields;
//                                else if (pars[c].Fields != null)
//                                    tt.Fields.AddRange(pars[c].Fields);

//                                if (tt.FunctionType == null)
//                                    tt.FunctionType = pars[c].FunctionType;
//                                else if (pars[c].FunctionType != null)
//                                    tt.FunctionType.AddRange(pars[c].FunctionType);

//                                pars[c] = tt;
//                            }

//                            c++;
//                        }
//                    }
//                    else
//                    {
//                        var tt = GetPatternType(e.Pattern, lc);

//                        if (pars[0] == null)
//                            pars[0] = tt;
//                        else
//                        {
//                            if (tt.Fields == null)
//                                tt.Fields = pars[0].Fields;
//                            else if (pars[0].Fields != null)
//                                tt.Fields.AddRange(pars[0].Fields);

//                            if (tt.FunctionType == null)
//                                tt.FunctionType = pars[0].FunctionType;
//                            else if (pars[0].FunctionType != null)
//                                tt.FunctionType.AddRange(pars[0].FunctionType);

//                            tt.Traits |= pars[0].Traits;
//                            pars[0] = tt;
//                        }
//                    }
//                }

//                var type = GetExpressionType(e.Expression, lc);

//                if (bodyType == null)
//                    bodyType = type;
//                else if (type.Traits != bodyType.Traits)
//                    bodyType = new TypeTag(type.Traits | (bodyType != null ? bodyType.Traits : ElaTraits.None), false);
//            }

//            var ret = new TypeTag(ElaTraits.Call, true);
//            ret.FunctionType = new List<TypeTag>();
//            ret.FunctionType.AddRange(pars);
//            ret.FunctionType.Add(bodyType);
//            return ret;
//        }


//        private TypeTag GetPatternType(ElaPattern pat, BindingScope scope)
//        {
//            if (pat == null)
//                return new TypeTag(ElaTraits.None, false);

//            switch (pat.Type)
//            {
//                case ElaNodeType.LiteralPattern:
//                    {
//                        var lit = ((ElaLiteralPattern)pat).Value;
//                        return new TypeTag(GetTraits(lit.LiteralType), true);
//                    }
//                case ElaNodeType.AsPattern:
//                    {
//                        var @as = (ElaAsPattern)pat;
//                        var type = GetPatternType(@as.Pattern, scope);
//                        scope.LocalBindings.Add(@as.Name, new Binding(@as.Name, type));
//                        return type;
//                    }
//                case ElaNodeType.FieldPattern:
//                    {
//                        var fld = (ElaFieldPattern)pat;
//                        var type = GetPatternType(fld.Value, scope);
//                        scope.LocalBindings.Add(fld.Name, new Binding(fld.Name, type));
//                        return type;
//                    }
//                case ElaNodeType.HeadTailPattern:
//                    {
//                        var ht = (ElaHeadTailPattern)pat;
//                        var c = 0;

//                        foreach (var p in ht.Patterns)
//                        {
//                            if (++c == ht.Patterns.Count &&
//                                p.Type == ElaNodeType.VariablePattern)
//                            {
//                                var v = (ElaVariablePattern)p;								
//                                scope.LocalBindings.Add(v.Name, new Binding(v.Name, new TypeTag(ElaTraits.Seq, false)));
//                            }
//                            else
//                                GetPatternType(p, scope);
//                        }

//                        return new TypeTag(ElaTraits.Seq, false);
//                    }
//                case ElaNodeType.NilPattern: return new TypeTag(ElaTraits.Seq, false);
//                case ElaNodeType.RecordPattern:
//                    {
//                        var rec = (ElaRecordPattern)pat;

//                        foreach (var f in rec.Fields)
//                            GetPatternType(f, scope);

//                        return new TypeTag(ElaTraits.FieldGet, false);
//                    }
//                case ElaNodeType.TuplePattern:
//                    {
//                        var tup = (ElaTuplePattern)pat;

//                        foreach (var e in tup.Patterns)
//                            GetPatternType(e, scope);

//                        return new TypeTag(ElaTraits.Get|ElaTraits.Ix|ElaTraits.Len, false);
//                    }
//                case ElaNodeType.UnitPattern:
//                    return new TypeTag(GetTraits(ElaTypeCode.Unit), true);
//                case ElaNodeType.VariablePattern:
//                    {
//                        var v = (ElaVariablePattern)pat;
//                        var type = new TypeTag(ElaTraits.None, false);						
//                        scope.LocalBindings.Add(v.Name, new Binding(v.Name, type));
//                        return type;
//                    }
//                case ElaNodeType.VariantPattern:
//                    {
//                        var var = (ElaVariantPattern)pat;
//                        var type = GetPatternType(var.Pattern, scope);
//                        return new TypeTag(ElaTraits.Tag | type.Traits, type.Complete);
//                    }
//                case ElaNodeType.CastPattern:
//                    {
//                        var cst = (ElaCastPattern)pat;
//                        return new TypeTag(GetTraits(cst.TypeAffinity), true);
//                    }
//                case ElaNodeType.IsPattern:
//                    {
//                        var isp = (ElaIsPattern)pat;
//                        return isp.Traits != ElaTraits.None ? new TypeTag(isp.Traits, false) :
//                            new TypeTag(GetTraits(isp.TypeAffinity), true);
//                    }
//                default:
//                    return TypeTag.Any();
//            }
//        }
//    }


//    class BindingScope
//    {
//        public BindingScope(BindingScope parent)
//        {
//            ParentScope = parent;
//            LocalBindings = new Dictionary<String,Binding>();
//            ChildScopes = new List<BindingScope>();
//        }

//        public Dictionary<String,Binding> LocalBindings { get; private set; }

//        public BindingScope ParentScope { get; private set; }

//        public List<BindingScope> ChildScopes { get; private set; }
//    }



//    class TypeTag
//    {
//        public static TypeTag Any() { return new TypeTag(ElaTraits.None, true); }
//        public static TypeTag Any(bool complete) { return new TypeTag(ElaTraits.None, complete); }
//        public static TypeTag Unit() { return new TypeTag(ElaTraits.Eq | ElaTraits.Show, true); }

//        public TypeTag(ElaTraits traits, bool complete)
//        {
//            Traits = traits;
//            Complete = complete;
//        }

//        public bool Complete { get; private set; }

//        public ElaTraits Traits { get; set; }

//        public List<Binding> Fields { get; set; }

//        public List<TypeTag> FunctionType { get; set; }

//        public override string ToString()
//        {
//            if (FunctionType != null)
//            {
//                var sb = new StringBuilder();
//                var c = 0;

//                foreach (var t in FunctionType)
//                {
//                    if (c++ > 0)
//                        sb.Append("->");

//                    sb.Append(t);
//                }

//                return sb.ToString();
//            }
//            else
//                return "(" + (Traits == ElaTraits.None ? "a" : Traits.ToString().Replace(" ", "")) + (Complete ? "" : "*") + ")";
//        }
//    }



//    class Binding
//    {
//        public Binding(string name, TypeTag type)
//        {
//            Name = name;
//            Type = type;
//        }

//        public string Name { get; private set; }

//        public TypeTag Type { get; private set; }

//        public BindingScope Scope { get; private set; }


//        public override string ToString()
//        {
//            var pref = Type.FunctionType != null ? "fun" : "let";
//            return String.Format("{0} {1} = {2}", pref, Name, Type);
//        }
//    }
//}
