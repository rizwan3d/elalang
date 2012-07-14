using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part is responsible for compilation of simple literal forms
    internal sealed partial class Builder
    {
        //Compiles a primitive literal value
        private ExprData CompilePrimitive(ElaPrimitive p, LabelMap map, Hints hints)
        {
            AddLinePragma(p);
            PushPrimitive(p.Value);            

            if ((hints & Hints.Left) == Hints.Left)
                AddValueNotUsed(p);

            return new ExprData(DataKind.VarType, -1);
        }

        //Pushes a primitive value. It can be a string, a char,
        //a 32-bit integer, a 64-bit integer, a 32-bit float, a 64-bit float,
        //a boolean or unit.
        private void PushPrimitive(ElaLiteralValue val)
        {
            switch (val.LiteralType)
            {
                case ElaTypeCode.String:
                    var str = val.AsString();

                    //String is added to the string table and indexed
                    //An empty string is pushed using special op code.
                    if (str.Length == 0)
                        cw.Emit(Op.Pushstr_0);
                    else
                        cw.Emit(Op.Pushstr, AddString(str));
                    break;
                case ElaTypeCode.Char:
                    cw.Emit(Op.PushCh, val.AsInteger());
                    break;
                case ElaTypeCode.Integer:
                    var v = val.AsInteger();

                    if (v == 0)
                        cw.Emit(Op.PushI4_0);
                    else
                        cw.Emit(Op.PushI4, v);
                    break;
                case ElaTypeCode.Long:
                    //64-bit long is pushed as two 32-bit integers
                    cw.Emit(Op.PushI4, val.GetData().I4_1);
                    cw.Emit(Op.PushI4, val.GetData().I4_2);
                    cw.Emit(Op.NewI8);
                    break;
                case ElaTypeCode.Single:
                    cw.Emit(Op.PushR4, val.GetData().I4_1);
                    break;
                case ElaTypeCode.Double:
                    //64-bit float is pushed as two 32-bit integers
                    cw.Emit(Op.PushI4, val.GetData().I4_1);
                    cw.Emit(Op.PushI4, val.GetData().I4_2);
                    cw.Emit(Op.NewR8);
                    break;
                case ElaTypeCode.Boolean:
                    cw.Emit(val.AsBoolean() ? Op.PushI1_1 : Op.PushI1_0);
                    break;
                default:
                    cw.Emit(Op.Pushunit);
                    break;
            }
        }

        //Compiles record literal
        private ExprData CompileRecord(ElaRecordLiteral p, LabelMap map, Hints hints)
        {
            AddLinePragma(p);
            cw.Emit(Op.Newrec, p.Fields.Count);

            for (var i = 0; i < p.Fields.Count; i++)
            {
                var f = p.Fields[i];
                CompileExpression(f.FieldValue, map, Hints.None);
                cw.Emit(Op.Pushstr, AddString(f.FieldName));
                cw.Emit(Op.Reccons, i);
            }

            if ((hints & Hints.Left) == Hints.Left)
                AddValueNotUsed(p);

            return new ExprData(DataKind.VarType, -1);
        }

        //Compiles list literal
        private ExprData CompileList(ElaListLiteral p, LabelMap map, Hints hints)
        {
            var len = p.Values.Count;
            AddLinePragma(p);
            cw.Emit(Op.Newlist);

            //If len is 0 than we have an empty (nil) list which is created by Newlist.
            for (var i = 0; i < len; i++)
            {
                CompileExpression(p.Values[p.Values.Count - i - 1], map, Hints.None);
                cw.Emit(Op.Gen);
            }

            if ((hints & Hints.Left) == Hints.Left)
                AddValueNotUsed(p);

            return new ExprData(DataKind.VarType, -1);
        }

        //Compiles tuple literal
        private ExprData CompileTuple(ElaTupleLiteral v, LabelMap map, Hints hints)
        {
            var pars = v.Parameters;

            //Optimize tuple creates for a case of pair (a single op code is used).
            if (pars.Count == 2)
            {
                CompileExpression(pars[0], map, Hints.None);
                CompileExpression(pars[1], map, Hints.None);
                AddLinePragma(v);
                cw.Emit(Op.Newtup_2);
            }
            else
            {
                AddLinePragma(v);
                cw.Emit(Op.Newtup, pars.Count);

                for (var i = 0; i < pars.Count; i++)
                {
                    CompileExpression(pars[i], map, Hints.None);
                    cw.Emit(Op.Tupcons, i);
                }
            }

            if ((hints & Hints.Left) == Hints.Left)
                AddValueNotUsed(v);

            return new ExprData(DataKind.VarType, -1);
        }
    }
}
