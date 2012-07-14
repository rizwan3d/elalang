using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Runtime;

namespace Ela.Compilation
{
    //This part contains compilation logic for built-in and user defined types.
    internal sealed partial class Builder
    {
        //Main method for type compilation
        private void CompileType(ElaNewtype v, LabelMap map)
        {
            //We need to obtain type code for a type
            var tc = -1;
            var sca = v.Extends ? AddVariable() : AddVariable("$$" + v.Name, v, ElaVariableFlags.None, -1);
                
            //A body may be null only if this is a built-in type
            if (!v.HasBody && v.Extends)
                AddError(ElaCompilerError.OnlyBuiltinTypeNoDefinition, v, v.Name);
            else if (!v.HasBody)
            {
                tc = (Int32)TCF.GetTypeCode(v.Name);

                //OK, type is no built-in, this cannot work
                if (tc == 0)
                    AddError(ElaCompilerError.OnlyBuiltinTypeNoDefinition, v, v.Name);

                //We add a special variable that contains a global type ID
                cw.Emit(Op.PushI4, tc);
                PopVar(sca);
            }

            //Type is already declared within the same module (types from different
            //modules can shadow each, so this is perfectly OK).
            if (!v.Extends && frame.InternalTypes.ContainsKey(v.Name))
                AddError(ElaCompilerError.TypeAlreadyDeclared, v, v.Name);
            else
            {
                if (v.Prefix != null && !v.Extends)
                    AddError(ElaCompilerError.InvalidTypeDefinition, v);

                if (!v.Extends)
                    frame.InternalTypes.Add(v.Name, tc);

                AddLinePragma(v);

                //Add a type var for a non built-in type
                if (v.HasBody)
                {
                    //Add a special variable with global type ID which will be calculated at run-time
                    if (v.Extends)
                        EmitSpecName(v.Prefix, "$$" + v.Name, v, ElaCompilerError.UndefinedType);
                    else
                        cw.Emit(Op.Typeid, AddString(v.Name));
                    
                    PopVar(sca);

                    for (var i = 0; i < v.Constructors.Count; i++)
                    {
                        var c = v.Constructors[i];
                        CompileConstructor(sca, c);
                    }
                }
                else
                    cw.Emit(Op.Nop);
            }

            if (v.And != null)
                CompileType(v.And, map);
        }

        //Generic compilation logic for a constructor, compiles both functions and constants.
        //Parameter sca is an address of variable that contains real type ID.
        private void CompileConstructor(int sca, ElaConstructor c)
        {
            if (c.Expressions.Count > 0)
                CompileConstructorFunction(c, sca);
            else
            {
                AddLinePragma(c);
                cw.Emit(Op.Pushunit);
                cw.Emit(Op.Ctorid, AddString(c.Name));
                PushVar(sca);
                cw.Emit(Op.Newtype);
                var a = AddVariable(c.Name, c, ElaVariableFlags.TypeFun, -1);
                PopVar(a);
            }

            var ab = AddVariable("$$$$" + c.Name, c, ElaVariableFlags.None, -1);
            cw.Emit(Op.Ctorid, AddString(c.Name));
            PopVar(ab);
            frame.InternalConstructors.Add(c.Name, -1);
        }

        //Compiles a type constructor
        private void CompileConstructorFunction(ElaConstructor cons, int sca)
        {
            Label funSkipLabel;
            int address;
            LabelMap newMap;
            var len = cons.Expressions.Count;

            CompileFunctionProlog(cons.Name, len, cons.Line, cons.Column, out funSkipLabel, out address, out newMap);

            //Compile function body
            var failLab = cw.DefineLabel();
            var succLab = cw.DefineLabel();
            
            var sys = new int[len];
            
            for (var i = 0; i < len; i++)
            {
                sys[i] = AddVariable();
                PopVar(sys[i]);

                var ce = cons.Expressions[i];

                if (ce.Type != ElaNodeType.NameReference &&
                    ce.Type != ElaNodeType.Placeholder)                    
                    CompilePattern(sys[i], ce, failLab);
            }

            cw.Emit(Op.Br, succLab);
            
            //Not very happy path, match failed, have to raise an exception
            cw.MarkLabel(failLab);
            cw.Emit(Op.Failwith, (Int32)ElaRuntimeError.MatchFailed);

            //Happy path, pattern match was OK now we just need
            //to push the passed argument and create a type instance
            cw.MarkLabel(succLab);
            cw.Emit(Op.Newtup, len);

            for (var i = 0; i < len; i++)
            {
                PushVar(sys[i]);
                cw.Emit(Op.Tupcons, i);
            }

            cw.Emit(Op.Ctorid, AddString(cons.Name));
            //Refering to captured name, need to recode its address
            PushVar((Byte.MaxValue & sca) + 1 | (sca << 8) >> 8);
            cw.Emit(Op.Newtype);

            CompileFunctionEpilog(cons.Name, len, address, funSkipLabel);
            var a = AddVariable(cons.Name, cons, ElaVariableFlags.TypeFun, -1);
            PopVar(a);
        }
    }
}
