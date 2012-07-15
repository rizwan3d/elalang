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
                var typeParams = new Dictionary<String,Int32>();

                //Type definition has type parameters that can be used by constructors
                if (v.HasTypeParams)
                {
                    for (var i = 0; i < v.TypeParams.Count; i++)
                    {
                        var p = v.TypeParams[i];

                        //Type parameters should be unique identifiers
                        if (typeParams.ContainsKey(p))
                            AddError(ElaCompilerError.DuplicateTypeParameter, v, p, v.Name);
                        else
                            typeParams.Add(p, 0);
                    }
                }

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
                        CompileConstructor(typeParams, sca, c);
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
        private void CompileConstructor(Dictionary<String,Int32> typeParams, int sca, ElaExpression exp)
        {
            //Constructor name
            var name = String.Empty;

            switch (exp.Type)
            {
                case ElaNodeType.NameReference:
                    {
                        var n = (ElaNameReference)exp;
                        name = n.Name;

                        if (!n.Uppercase)
                            AddError(ElaCompilerError.InvalidConstructor, n, FormatNode(n));
                        else
                            CompileConstructorConstant(n, sca);
                    }
                    break;
                case ElaNodeType.Juxtaposition:
                    {
                        var n = (ElaJuxtaposition)exp;

                        if (n.Target.Type == ElaNodeType.NameReference)
                        {
                            var m = (ElaNameReference)n.Target;
                            name = m.Name;

                            if (m.Uppercase || (Format.IsSymbolic(m.Name) && n.Parameters.Count <= 2))
                            {
                                CompileConstructorFunction(typeParams, m.Name, n, sca);
                                break;
                            }   
                        }

                        AddError(ElaCompilerError.InvalidConstructor, exp, FormatNode(exp));
                    }
                    break;
                default:
                    AddError(ElaCompilerError.InvalidConstructor, exp, FormatNode(exp));
                    break;
            }

            var ab = AddVariable("$$$$" + name, exp, ElaVariableFlags.None, -1);
            cw.Emit(Op.Ctorid, AddString(name));
            PopVar(ab);
            frame.InternalConstructors.Add(name, -1);
        }

        //Compiles a simple parameterless constructor
        private void CompileConstructorConstant(ElaNameReference exp, int sca)
        {
            AddLinePragma(exp);
            cw.Emit(Op.Pushunit);
            cw.Emit(Op.Ctorid, AddString(exp.Name));
            PushVar(sca);
            cw.Emit(Op.Newtype);
            var a = AddVariable(exp.Name, exp, ElaVariableFlags.TypeFun, -1);
            PopVar(a);
        }

        //Compiles a type constructor
        private void CompileConstructorFunction(Dictionary<String,Int32> dict, string name, ElaJuxtaposition juxta, int sca)
        {
            Label funSkipLabel;
            int address;
            LabelMap newMap;
            var len = juxta.Parameters.Count;
            var typeParams = new Dictionary<String,Int32>();

            foreach (var kv in dict)
                typeParams.Add(kv.Key, -1);

            AddLinePragma(juxta);
            CompileFunctionProlog(name, len, juxta.Line, juxta.Column, out funSkipLabel, out address, out newMap);

            //Compile function body
            var failLab = cw.DefineLabel();
            var succLab = cw.DefineLabel();
            
            var sys = new int[len];
            
            for (var i = 0; i < len; i++)
            {
                var ce = juxta.Parameters[i];
                sys[i] = AddVariable();
                PopVar(sys[i]);

                if (ce.Type == ElaNodeType.NameReference)
                {
                    var n = (ElaNameReference)ce;
                    var checkVar = default(Int32);

                    //First we check if a given name is a type parameter. If it is
                    //we consider it to be a direct type name
                    if (!typeParams.TryGetValue(n.Name, out checkVar))
                    {
                        PushVar(sys[i]);
                        EmitSpecName(null, "$$" + n.Name, n, ElaCompilerError.UndefinedType);
                        cw.Emit(Op.Ctypei);
                        cw.Emit(Op.Brfalse, failLab);
                    }
                    else
                    {
                        //If this is the first occurence of a variable we only need to memorize
                        //its address, otherwise we need to check its type.
                        if (checkVar == -1)
                            typeParams[n.Name] = sys[i];
                        else
                        {
                            PushVar(checkVar);
                            PushVar(sys[i]);
                            cw.Emit(Op.Ctype);
                            cw.Emit(Op.Brfalse, failLab);
                        }
                    }
                }
                else if (ce.Type == ElaNodeType.FieldReference)
                {
                    //A type can be also prefixed with a module alias. We process this case here
                    var n = (ElaFieldReference)ce;

                    if (n.TargetObject.Type != ElaNodeType.NameReference)
                        AddError(ElaCompilerError.InvalidConstructorParameter, juxta, FormatNode(ce), name);

                    PushVar(sys[i]);
                    EmitSpecName(n.TargetObject.GetName(), "$$" + n.FieldName, n, ElaCompilerError.UndefinedType);
                    cw.Emit(Op.Ctypei);
                    cw.Emit(Op.Brfalse, failLab);
                }
                else
                    AddError(ElaCompilerError.InvalidConstructorParameter, juxta, FormatNode(ce), name);                
                
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

            cw.Emit(Op.Ctorid, AddString(name));
            //Refering to captured name, need to recode its address
            PushVar((Byte.MaxValue & sca) + 1 | (sca << 8) >> 8);
            cw.Emit(Op.Newtype);

            CompileFunctionEpilog(name, len, address, funSkipLabel);
            var a = AddVariable(name, juxta, ElaVariableFlags.TypeFun, -1);
            PopVar(a);
        }
    }
}
