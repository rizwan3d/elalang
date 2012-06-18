using System;
using Ela.CodeModel;
using Ela.Runtime;

namespace Ela.Compilation
{
    //This part is responsible for function compilation.
	internal sealed partial class Builder
	{
        //Main method used to compile functions. Can compile regular functions, 
        //functions in-place (FunFlag.Inline), as lazy sections (FunFlag.Lazy) and type constructors (FunFlag.Newtype).
		private void CompileFunction(ElaFunctionLiteral dec, FunFlag flag)
		{
			var pars = dec.ParameterCount;

            //Don't generate debug info when a function is compiled inline.
			if (flag != FunFlag.Inline)
				StartFun(dec.Name, pars);

			var funSkipLabel = Label.Empty;

			var map = new LabelMap();
			var startLabel = cw.DefineLabel();

            //Functions are always compiled in place, e.g. when met. Therefore a 'goto'
            //instruction is emitted to skip through function definition. This instruction
            //is obviously not needed when a function is inlined.
            if (flag != FunFlag.Inline)
			{
				funSkipLabel = cw.DefineLabel();
				cw.Emit(Op.Br, funSkipLabel);
			}

            //FunStart label is needed for tail recursive calls when we emit a 'goto' 
            //instead of an actual function call.
			map.FunStart = startLabel;

            //Preserve some information about a function we're currently in.
			map.FunctionName = dec.Name;
			map.FunctionParameters = pars;
			map.FunctionScope = CurrentScope;
			map.InlineFunction = flag == FunFlag.Inline;

            //No need in jump label for an inlined function.
			if (flag != FunFlag.Inline)
				cw.MarkLabel(startLabel);

            //We start a real (VM based) lexical scope for a regular function,
            //and a compiler processed lexical scope for inlined function.
			StartScope(flag != FunFlag.Inline, dec.Body.Line, dec.Body.Column);

            //StartSection create a real lexical scope - not needed when inlined.
			if (flag != FunFlag.Inline)
				StartSection();
			
			AddLinePragma(dec);

            var address = cw.Offset;
            			
            //Lazy section doesn't have parameters.
			if (flag != FunFlag.Lazy)
				CompileParameters(dec, map, flag);

            //When a function has a single body use a slightly simplified compilation logic.
            //Otherwise compile a whole function as a pattern match expression.
            if (dec.Body.Entries.Count > 1)
			{
                var t = (ElaTupleLiteral)dec.Body.Expression;
                
                //Parser always wraps function parameters in a tuple even if a function
                //has a single parameter. We need to avoid this redundancy.
				var ex = t.Parameters.Count == 1 ? t.Parameters[0] : t;
				CompileMatch(dec.Body, ex, map, Hints.Tail | Hints.Scope | Hints.FunBody);
			}
			else 
			{                
                //As soon as we are using a simplified compilation technique we have to
                //process 'where' binding by ourselves.
                if (dec.Body.Entries[0].Where != null)
					CompileExpression(dec.Body.Entries[0].Where, map, Hints.Left);

                //We don't need a 'tail situation' for lazy and newtype.
                var eh = flag == FunFlag.None || flag == FunFlag.Inline ? Hints.Scope | Hints.Tail : Hints.Scope;
				CompileExpression(dec.Body.Entries[0].Expression, map, eh);
			}

            //This logic created a function (by finally emitting Newfun).
            //Obviously not needed for inlined function.
			if (flag != FunFlag.Inline)
			{
				var funHandle = frame.Layouts.Count;
				var ss = EndFun(funHandle);
				frame.Layouts.Add(new MemoryLayout(currentCounter, ss, address));
				EndScope();
				EndSection();

                //For a type constructor function the last instruction
                //in a function should be a Newtype op code.
                if (flag == FunFlag.Newtype)
                    cw.Emit(Op.Newtype, AddString(typeName));

				cw.Emit(Op.Ret);
				cw.MarkLabel(funSkipLabel);

				AddLinePragma(dec);

                //Function is constructed
				cw.Emit(Op.PushI4, pars);
				cw.Emit(Op.Newfun, funHandle);
			}
		}
        
        //Here we compile a function parameter list
		private void CompileParameters(ElaFunctionLiteral fun, LabelMap map, FunFlag flag)
		{
            //When a function has a single body we use a simplified compilication technique
            //(senerates a slightly faster code).
            if (fun.Body.Entries.Count == 1)
			{
				var fe = fun.Body.Entries[0];
				var seq = fe.Pattern.Type == ElaNodeType.PatternGroup ? (ElaPatternGroup)fe.Pattern : null;
				var parCount = seq != null ? seq.Patterns.Count : 1;
				var ng = fe.Guard == null;

                //Here we process all patterns manually and emitting a faster code for regular
                //variable and default patterns.
				for (var i = 0; i < parCount; i++)
				{
					var e = parCount > 1 ? seq.Patterns[i] : fe.Pattern;

					if (ng && e.Type == ElaNodeType.VariablePattern)
						AddParameter((ElaVariablePattern)e);
					else if (ng && e.Type == ElaNodeType.DefaultPattern)
						cw.Emit(Op.Pop);
					else
					{
						var addr = AddVariable();
                        PopVar(addr);
						var nextLab = cw.DefineLabel();
						var skipLab = cw.DefineLabel();
						CompilePattern(addr, null, e, map, nextLab, ElaVariableFlags.None, Hints.FunBody);
						cw.Emit(Op.Br, skipLab);
						cw.MarkLabel(nextLab);
						cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
						cw.MarkLabel(skipLab);
						cw.Emit(Op.Nop);
					}
				}

                //Generates code for a guard if it is present.
				if (fe.Guard != null)
				{
					var next = cw.DefineLabel();
					CompileExpression(fe.Guard, map, Hints.Scope);
					cw.Emit(Op.Brtrue, next);
					cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);
					cw.MarkLabel(next);
					cw.Emit(Op.Nop);
				}
			}
            else
            {
                //Regular compilation logic for multiple body function. In this case parameters
                //are presented through hidden variables (with $ prefixed). These variables are created
                //to capture values on a stack and match against patterns (when compiling the rest of
                //a function as a pattern matching construct).
                var t = (ElaTupleLiteral)fun.Body.Expression;

                for (var i = 0; i < t.Parameters.Count; i++)
                {
                    var v = (ElaVariableReference)t.Parameters[i];
                    var addr = AddVariable(v.VariableName, v,
                        v.VariableName[0] == '$' ?
                        (ElaVariableFlags.SpecialName | ElaVariableFlags.Parameter) : ElaVariableFlags.Parameter, -1);
                    PopVar(addr);
                }
            }
		}

        //Adds a parameter. Parameters can shadow each other.
        private void AddParameter(ElaVariablePattern exp)
        {
            CurrentScope.Locals.Remove(exp.Name); //Parameters can hide each other
            var addr = AddVariable(exp.Name, exp, ElaVariableFlags.Parameter, -1);
            PopVar(addr);
        }

        //This methods tries to optimize lazy section. It would only work when a lazy
        //section if a function application that result in saturation (no partial applications)
        //allowed. In such a case this method eliminates "double" function call (which would be
        //the result of a regular compilation logic). If this method fails than regular compilation
        //logic is used.
		private bool TryOptimizeLazy(ElaLazyLiteral lazy, LabelMap map, Hints hints)
		{
            var body = default(ElaExpression);

            //Only function application is accepted
			if ((body = lazy.Body.Entries[0].Expression).Type != ElaNodeType.FunctionCall)
				return false;

			var funCall = (ElaFunctionCall)body;

            //If a target is not a variable we can't check what is actually called
			if (funCall.Target.Type != ElaNodeType.VariableReference)
				return false;

			var varRef = (ElaVariableReference)funCall.Target;
			var scopeVar = GetVariable(varRef.VariableName, varRef.Line, varRef.Column);
			var len = funCall.Parameters.Count;

            //If a target is not function we can't optimize it
            if ((scopeVar.VariableFlags & ElaVariableFlags.Function) != ElaVariableFlags.Function)
                return false;

            //Only saturation case is optimized
			if (scopeVar.Data != funCall.Parameters.Count)
				return false;

			for (var i = 0; i < len; i++)
				CompileExpression(funCall.Parameters[len - i - 1], map, Hints.None);

			var sl = len - 1;
			AddLinePragma(varRef);
			PushVar(scopeVar);

            //We partially apply function and create a new function
			for (var i = 0; i < sl; i++)
				cw.Emit(Op.Call);

			AddLinePragma(lazy);

            //LazyCall uses a function provided to create a thunk
            //and remembers last function arguments as ElaFunction.LastParameter
			cw.Emit(Op.LazyCall, len);
			return true;
		}
	}
}