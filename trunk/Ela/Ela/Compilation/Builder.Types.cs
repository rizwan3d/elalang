using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Runtime;

namespace Ela.Compilation
{
    //This part contains compilation logic for built-in and user defined types.
    internal sealed partial class Builder
    {
        private Dictionary<String,ElaNewtype> headers = new Dictionary<String,ElaNewtype>();

        //An entry method for type compilation. Ensures that types ('type') are
        //always compiled before type extensions ('data').
        private void CompileTypes(ElaNewtype v, LabelMap map)
        {
            CompileHeaders(v);
            CompileTypeOnly(v, map);
            CompileDataOnly(v, map);

            foreach (var kv in headers)
                AddError(ElaCompilerError.TypeHeaderNotConnected, kv.Value, kv.Value.Name);
        }

        //Builds a list of attribute headers for types
        private void CompileHeaders(ElaNewtype v)
        {
            if (v.Flags != ElaVariableFlags.None)
            {
                if (headers.ContainsKey(v.Prefix + "." + v.Name))
                    AddError(ElaCompilerError.TypeHeaderNotConnected, v, v.Name);
                else
                    headers.Add(v.Prefix + "." + v.Name, v);
            }

            if (v.And != null)
                CompileHeaders(v.And);
        }


        //This method only compiles types declared through 'type' keyword
        //and not type extensions ('data' declarations).
        private void CompileTypeOnly(ElaNewtype v, LabelMap map)
        {
            if (!v.Extends && v.Flags == ElaVariableFlags.None)
                CompileTypeBody(v, map);

            if (v.And != null)
                CompileTypeOnly(v.And, map);
        }

        //This method only compiles type extensions (declared through 'data').
        private void CompileDataOnly(ElaNewtype v, LabelMap map)
        {
            if (v.Extends && v.Flags == ElaVariableFlags.None)
                CompileTypeBody(v, map);

            if (v.And != null)
                CompileDataOnly(v.And, map);
        }

        //Main method for type compilation
        private void CompileTypeBody(ElaNewtype v, LabelMap map)
        {
            //We need to obtain type code for a type
            var tc = -1;
            var sca = -1;
            var hd = default(ElaNewtype);
            var flags = ElaVariableFlags.None;
            var isList = false;

            if (headers.TryGetValue(v.Prefix + "." + v.Name, out hd))
            {
                flags = hd.Flags;
                headers.Remove(v.Prefix + "." + v.Name);
            }
                
            //A body may be null only if this is a built-in type
            if (!v.HasBody && v.Extends)
                AddError(ElaCompilerError.OnlyBuiltinTypeNoDefinition, v, v.Name);
            else if (!v.HasBody || (isList = IsList(v, flags)))
            {
                tc = (Int32)TCF.GetTypeCode(v.Name);                
                sca = AddVariable("$$" + v.Name, v, flags, tc);

                //OK, type is no built-in, this cannot work
                if (tc == 0)
                    AddError(ElaCompilerError.OnlyBuiltinTypeNoDefinition, v, v.Name);

                //We add a special variable that contains a global type ID
                cw.Emit(Op.PushI4, tc);
                PopVar(sca);
            }
            else
                sca = v.Extends ? AddVariable() : AddVariable("$$" + v.Name, v, flags, -1);

            //Type is already declared within the same module (types from different
            //modules can shadow each, so this is perfectly OK).
            if (!v.Extends && frame.InternalTypes.ContainsKey(v.Name))
            {
                AddError(ElaCompilerError.TypeAlreadyDeclared, v, v.Name);
                frame.InternalTypes.Remove(v.Name);
            }
            
           if (v.Prefix != null && !v.Extends)
                AddError(ElaCompilerError.InvalidTypeDefinition, v);

            if (!v.Extends)
                frame.InternalTypes.Add(v.Name, tc);

            AddLinePragma(v);
            var typeParams = new Dictionary<String,Int32>();

            //Type definition has type parameters that can be used by constructors
            if (v.HasTypeParams && !isList)
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
            if (v.HasBody && !isList)
            {
                //Add a special variable with global type ID which will be calculated at run-time
                if (v.Extends)
                {
                    var sv = EmitSpecName(v.Prefix, "$$" + v.Name, v, ElaCompilerError.UndefinedType);

                    //If this is a built-in type, than a variable contains data with type ID
                    //We capture this case here, thus disallowing built-in types.
                    if (sv != -1)
                        AddError(ElaCompilerError.UnableExtendBuiltin, v, v.Name);
                }
                else
                    cw.Emit(Op.Typeid, AddString(v.Name));
                
                PopVar(sca);

                for (var i = 0; i < v.Constructors.Count; i++)
                {
                    var c = v.Constructors[i];
                    CompileConstructor(typeParams, sca, c, flags);
                }
            }
            else if (isList)
            {
                //This is a special case of a built-in type - a linked list - which
                //definition is fully written to have an ability to specify names for constructors.
                var nil = v.Constructors[0].Type == ElaNodeType.NameReference ?
                    v.Constructors[0] : v.Constructors[1];
                var cons = v.Constructors[0].Type == ElaNodeType.Juxtaposition ?
                    v.Constructors[0] : v.Constructors[1];

                var nilVar = AddVariable(nil.GetName(), nil, ElaVariableFlags.TypeFun|flags, -1);
                cw.Emit(Op.Newlist);
                PopVar(nilVar);

                var consName = cons.GetName();
                var consVar = AddVariable(consName, cons, ElaVariableFlags.TypeFun|ElaVariableFlags.Builtin, (Int32)ElaBuiltinKind.Cons);
                CompileBuiltin(ElaBuiltinKind.Cons, cons, map, consName);
                PopVar(consVar);                
            }
            else
                cw.Emit(Op.Nop);
        }

        //Generic compilation logic for a constructor, compiles both functions and constants.
        //Parameter sca is an address of variable that contains real type ID.
        private void CompileConstructor(Dictionary<String,Int32> typeParams, int sca, ElaExpression exp, ElaVariableFlags flags)
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
                            CompileConstructorConstant(n, sca, flags);
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
                                CompileConstructorFunction(typeParams, m.Name, n, sca, flags);
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

            var ab = AddVariable("$$$$" + name, exp, flags, -1);
            cw.Emit(Op.Ctorid, AddString(name));
            PopVar(ab);
            frame.InternalConstructors.Add(name, -1);
        }

        //Compiles a simple parameterless constructor
        private void CompileConstructorConstant(ElaNameReference exp, int sca, ElaVariableFlags flags)
        {
            AddLinePragma(exp);
            cw.Emit(Op.Pushunit);
            cw.Emit(Op.Ctorid, AddString(exp.Name));
            PushVar(sca);
            cw.Emit(Op.Newtype);
            var a = AddVariable(exp.Name, exp, ElaVariableFlags.TypeFun|flags, -1);
            PopVar(a);
        }

        //Compiles a type constructor
        private void CompileConstructorFunction(Dictionary<String,Int32> dict, string name, ElaJuxtaposition juxta, int sca, ElaVariableFlags flags)
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
            var a = AddVariable(name, juxta, ElaVariableFlags.TypeFun|flags, -1);
            PopVar(a);
        }

        //Checks if an algebraic type is actually a list which implementation can be optimized.
        private bool IsList(ElaNewtype t, ElaVariableFlags flags)
        {
            if (t.Flags != ElaVariableFlags.None
                || t.Name != "list"
                || t.Extends
                || !t.HasTypeParams
                || t.TypeParams.Count != 1)
                return false;

            var typeParam = t.TypeParams[0];

            return  t.Constructors.Count == 2 &&
                (t.Constructors[0].Type == ElaNodeType.NameReference && IsCons(t.Constructors[1], typeParam))
                || (t.Constructors[1].Type == ElaNodeType.NameReference && IsCons(t.Constructors[0], typeParam));
        }

        //Checks if a constructor is actually a list constructor which implementation can be optimized.
        private bool IsCons(ElaExpression exp, string typeParam)
        {
            if (exp.Type != ElaNodeType.Juxtaposition)
                return false;

            var j = (ElaJuxtaposition)exp;

            return j.Parameters.Count == 2 && j.Parameters[0].Type == ElaNodeType.NameReference
                && j.Parameters[0].GetName() == typeParam && j.Parameters[1].GetName() == "list";
        }
    }
}
