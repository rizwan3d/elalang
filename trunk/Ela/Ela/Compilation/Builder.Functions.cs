using System;
using Ela.CodeModel;
using Ela.Runtime;

namespace Ela.Compilation
{
    //This part is responsible for function compilation.
	internal sealed partial class Builder
	{
        //Main method used to compile functions. Can compile regular named functions, 
        //named functions in-place (FunFlag.Inline) and type constructors (FunFlag.Newtype).
		private void CompileFunction(ElaEquation dec)
		{
            var fc = (ElaJuxtaposition)dec.Left;
			var pars = fc.Parameters.Count;

            //Target can be null in a case of an anonymous function (lambda)
            var name = fc.Target != null ? fc.Target.GetName() : String.Empty;

            StartFun(name, pars);
			var funSkipLabel = Label.Empty;
			var map = new LabelMap();
			var startLabel = cw.DefineLabel();

            //Functions are always compiled in place, e.g. when met. Therefore a 'goto'
            //instruction is emitted to skip through function definition.
            funSkipLabel = cw.DefineLabel();
	        cw.Emit(Op.Br, funSkipLabel);

            //FunStart label is needed for tail recursive calls when we emit a 'goto' 
            //instead of an actual function call.
			map.FunStart = startLabel;

            //Preserve some information about a function we're currently in.
			map.FunctionName = name;
			map.FunctionParameters = pars;
			map.FunctionScope = CurrentScope;
			
            cw.MarkLabel(startLabel);

            //We start a real (VM based) lexical scope for a function.
			StartScope(true, dec.Right.Line, dec.Right.Column);

            //StartSection create a real lexical scope.
			StartSection();

            //If this a 'type member' function we can specify a tail hint because
            //it could cause a generation of Callt when we need to construct a type
            //(using Newtype) as the very last instruction in such a function.
            var hints = Hints.Scope | (dec.AssociatedType != null ? Hints.None : Hints.Tail);

			AddLinePragma(dec);
            var address = cw.Offset;
            CompileFunctionMatch(pars, dec, map, hints);

            //This logic creates a function (by finally emitting Newfun).
			var funHandle = frame.Layouts.Count;
			var ss = EndFun(funHandle);
			frame.Layouts.Add(new MemoryLayout(currentCounter, ss, address));
			EndScope();
			EndSection();

            //For a type constructor function the last instruction
            //in a function should be a Newtype op code.
            if (dec.AssociatedType != null)
                cw.Emit(Op.Newtype, AddString(dec.AssociatedType));

			cw.Emit(Op.Ret);
			cw.MarkLabel(funSkipLabel);

			AddLinePragma(dec);

            //Function is constructed
			cw.Emit(Op.PushI4, pars);
			cw.Emit(Op.Newfun, funHandle);
		}

        //Used to compile an anonymous function (lambda). This function returns a number of parameters 
        //in compiled lambda.
        private int CompileLambda(ElaEquation bid)
        {
            var fc = new ElaJuxtaposition();

            //Lambda is parsed as a an application of a lambda operator, e.g.
            //expr -> expr, therefore it needs to be transformed in order to be
            //able to use existing match compilation logic.
            if (bid.Left.Type == ElaNodeType.Juxtaposition)
            {
                var f = (ElaJuxtaposition)bid.Left;
                fc.Parameters.Add(f.Target);
                fc.Parameters.AddRange(f.Parameters);
            }
            else
                fc.Parameters.Add(bid.Left);

            bid.Left = fc;
            var parLen = fc.Parameters.Count;
            StartFun(null, parLen);

            var funSkipLabel = cw.DefineLabel();
            var map = new LabelMap();
            cw.Emit(Op.Br, funSkipLabel);

            //We start a real (VM based) lexical scope for a function.
            StartScope(true, bid.Right.Line, bid.Right.Column);
            StartSection();

            var address = cw.Offset;
            CompileFunctionMatch(parLen, bid, map, Hints.Scope | Hints.Tail);

            var funHandle = frame.Layouts.Count;
            var ss = EndFun(funHandle);
            frame.Layouts.Add(new MemoryLayout(currentCounter, ss, address));
            EndScope();
            EndSection();

            cw.Emit(Op.Ret);
            cw.MarkLabel(funSkipLabel);

            AddLinePragma(bid);

            //Function is constructed
            cw.Emit(Op.PushI4, parLen);
            cw.Emit(Op.Newfun, funHandle);
            return parLen;
        }

        //Generates the first part of the code needed to create a simple argument function. After
        //calling this method one should compile an expression that will become a function body.
        private void CompileFunctionProlog(string name, int pars, int line, int col, out Label funSkipLabel, out int address, out LabelMap newMap)
        {
            if (name != null)
                StartFun(name, pars);

            StartSection();
            StartScope(true, line, col);
            cw.StartFrame(pars);
            funSkipLabel = cw.DefineLabel();
            cw.Emit(Op.Br, funSkipLabel);
            address = cw.Offset;
            newMap = new LabelMap();
            newMap.FunctionScope = CurrentScope;
            newMap.FunctionParameters = pars;
        }

        //Generates the last past of a simple function by emitting Ret, initializing function and creating
        //a new function through Newfun. This method should be called right after compiling an expression
        //that should be a body of a function.
        private void CompileFunctionEpilog(string name, int pars, int address, Label funSkipLabel)
        {
            cw.Emit(Op.Ret);
            var ff = 0;

            if (name != null)
                ff = EndFun(frame.Layouts.Count);
            else
                ff = cw.FinishFrame();

            frame.Layouts.Add(new MemoryLayout(currentCounter, ff, address));
            EndSection();
            EndScope();
            cw.MarkLabel(funSkipLabel);
            cw.Emit(Op.PushI4, pars);
            cw.Emit(Op.Newfun, frame.Layouts.Count - 1);
        }
	}
}