using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part contains compilation logic of built-in functions. Normally these are
    //functions that correspond directly to Ela machine op codes.
    internal sealed partial class Builder
    {
        //Compiles built-in as function in place. It is compiled in such a manner each time
        //it is referenced. But normally its body is just one or two op codes, so this is not a problem.
        private void CompileBuiltin(ElaBuiltinKind kind, ElaExpression exp, LabelMap map)
        {
            StartSection();

            //Here we determine the number of parameters based on the function kind.
            var pars = BuiltinParams(kind);
            cw.StartFrame(pars);
            var funSkipLabel = cw.DefineLabel();
            cw.Emit(Op.Br, funSkipLabel);
            var address = cw.Offset;
            pdb.StartFunction(map.BuiltinName, address, pars);

            //Gets the actual code for built-in
            CompileBuiltinInline(kind, exp, map, Hints.None);

            cw.Emit(Op.Ret);
            frame.Layouts.Add(new MemoryLayout(currentCounter, cw.FinishFrame(), address));
            EndSection();
            pdb.EndFunction(frame.Layouts.Count - 1, cw.Offset);

            cw.MarkLabel(funSkipLabel);
            cw.Emit(Op.PushI4, pars);
            cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
        }
        
        //This method can be called directly when a built-in is inlined.
        private void CompileBuiltinInline(ElaBuiltinKind kind, ElaExpression exp, LabelMap map, Hints hints)
        {
            switch (kind)
            {
                case ElaBuiltinKind.Readf:
                    cw.Emit(Op.Read);
                    break;
                case ElaBuiltinKind.RecField:
                    cw.Emit(Op.Recfld);
                    break;
                case ElaBuiltinKind.ForwardPipe:
                    cw.Emit(Op.Swap);
                    cw.Emit(Op.Call);
                    break;
                case ElaBuiltinKind.BackwardPipe:
                    cw.Emit(Op.Call);
                    break;
                case ElaBuiltinKind.Cast:
                    cw.Emit(Op.Cast);
                    break;
                case ElaBuiltinKind.Gettag:
                    cw.Emit(Op.Gettag);
                    break;
                case ElaBuiltinKind.Untag:
                    cw.Emit(Op.Untag);
                    break;
                case ElaBuiltinKind.Head:
                    cw.Emit(Op.Head);
                    break;
                case ElaBuiltinKind.Tail:
                    cw.Emit(Op.Tail);
                    break;
                case ElaBuiltinKind.IsNil:
                    cw.Emit(Op.Isnil);
                    break;
                case ElaBuiltinKind.Negate:
                    cw.Emit(Op.Neg);
                    break;
                case ElaBuiltinKind.Succ:
                    cw.Emit(Op.Succ);
                    break;
                case ElaBuiltinKind.Pred:
                    cw.Emit(Op.Pred);
                    break;
                case ElaBuiltinKind.Type:
                    cw.Emit(Op.Type);
                    break;
                case ElaBuiltinKind.Length:
                    cw.Emit(Op.Len);
                    break;
                case ElaBuiltinKind.Force:
                    cw.Emit(Op.Force);
                    break;
                case ElaBuiltinKind.Not:
                    cw.Emit(Op.Not);
                    break;
                case ElaBuiltinKind.Flip:
                    cw.Emit(Op.Flip);
                    break;
                case ElaBuiltinKind.Nil:
                    cw.Emit(Op.Nil);
                    break;
                case ElaBuiltinKind.Showf:
                    cw.Emit(Op.Show);
                    break;
                case ElaBuiltinKind.Concat:
                    cw.Emit(Op.Concat);
                    break;
                case ElaBuiltinKind.Add:
                    cw.Emit(Op.Add);
                    break;
                case ElaBuiltinKind.Divide:
                    cw.Emit(Op.Div);
                    break;
                case ElaBuiltinKind.Multiply:
                    cw.Emit(Op.Mul);
                    break;
                case ElaBuiltinKind.Power:
                    cw.Emit(Op.Pow);
                    break;
                case ElaBuiltinKind.Remainder:
                    cw.Emit(Op.Rem);
                    break;
                case ElaBuiltinKind.Subtract:
                    cw.Emit(Op.Sub);
                    break;
                case ElaBuiltinKind.ShiftRight:
                    cw.Emit(Op.Shr);
                    break;
                case ElaBuiltinKind.ShiftLeft:
                    cw.Emit(Op.Shl);
                    break;
                case ElaBuiltinKind.Greater:
                    cw.Emit(Op.Cgt);
                    break;
                case ElaBuiltinKind.Lesser:
                    cw.Emit(Op.Clt);
                    break;
                case ElaBuiltinKind.Equal:
                    cw.Emit(Op.Ceq);
                    break;
                case ElaBuiltinKind.NotEqual:
                    cw.Emit(Op.Cneq);
                    break;
                case ElaBuiltinKind.GreaterEqual:
                    cw.Emit(Op.Cgteq);
                    break;
                case ElaBuiltinKind.LesserEqual:
                    cw.Emit(Op.Clteq);
                    break;
                case ElaBuiltinKind.BitwiseAnd:
                    cw.Emit(Op.AndBw);
                    break;
                case ElaBuiltinKind.BitwiseOr:
                    cw.Emit(Op.OrBw);
                    break;
                case ElaBuiltinKind.BitwiseXor:
                    cw.Emit(Op.Xor);
                    break;
                case ElaBuiltinKind.Cons:
                    cw.Emit(Op.Cons);
                    break;
                case ElaBuiltinKind.BitwiseNot:
                    cw.Emit(Op.NotBw);
                    break;
                case ElaBuiltinKind.Get:
                    cw.Emit(Op.Pushelem);
                    break;
            }
        }

        //Determines the number of built-in parameters based on its kind.
        private int BuiltinParams(ElaBuiltinKind kind)
        {
            if (kind == ElaBuiltinKind.Readf)
                return 3;

            return kind >= ElaBuiltinKind.Showf ? 2 : 1;
        }
    }
}
