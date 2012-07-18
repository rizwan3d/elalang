using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Runtime;

namespace Ela.Compilation
{
    //This part contains compilation logic for built-in and user defined types.
    internal sealed partial class Builder
    {
        //An entry method for type compilation. Ensures that types ('type') are
        //always compiled before type extensions ('data').
        private void CompileTypes(ElaNewtype v, LabelMap map)
        {
            CompileHeaders(v);
            CompileTypeOnly(v, map);
            CompileDataOnly(v, map);
        }

        //Builds a list of attribute headers for types
        private void CompileHeaders(ElaNewtype v)
        {
            var t = v;
            var oldt = default(ElaNewtype);

            while (t != null)
            {
                if (t.Header)
                {
                    if (oldt == null || t.Extends != oldt.Extends)
                        AddError(ElaCompilerError.TypeHeaderNotConnected, t, t.Name);
                    else
                        oldt.Flags = t.Flags;
                }
                else
                    oldt = t;

                t = t.And;
            }
        }
        
        //This method only compiles types declared through 'type' keyword
        //and not type extensions ('data' declarations).
        private void CompileTypeOnly(ElaNewtype v, LabelMap map)
        {
            if (!v.Extends && !v.Header)
                CompileTypeBody(v, map);

            if (v.And != null)
                CompileTypeOnly(v.And, map);
        }

        //This method only compiles type extensions (declared through 'data').
        private void CompileDataOnly(ElaNewtype v, LabelMap map)
        {
            if (v.Extends && !v.Header)
                CompileTypeBody(v, map);

            if (v.And != null)
                CompileDataOnly(v.And, map);
        }

        //Main method for type compilation
        private void CompileTypeBody(ElaNewtype v, LabelMap map)
        {
            //We need to obtain type typeId for a type
            var tc = -1;
            var sca = -1;
            var flags = v.Flags;
            var isList = false;
                
            //A body may be null only if this is a built-in type
            if (!v.HasBody && v.Extends)
                AddError(ElaCompilerError.ExtendsNoDefinition, v, v.Name);
            else if (!v.HasBody || (isList = IsList(v, flags)))
            {
                tc = (Int32)TCF.GetTypeCode(v.Name);
                tc = tc == 0 ? -1 : tc;
                sca = AddVariable("$$" + v.Name, v, flags, tc);

                //OK, type is built-in
                if (tc > 0)
                {
                    //We add a special variable that contains a global type ID
                    cw.Emit(Op.PushI4, tc);
                    PopVar(sca);
                }
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

            //-1 mean "this" module (considered by default).
            var typeModuleId = -1;

            //Add a type var for a non built-in type with a body
            if (tc == -1 && !isList)
            {
                //Add a special variable with global type ID which will be calculated at run-time
                if (v.Extends)
                {
                    var sv = EmitSpecName(v.Prefix, "$$" + v.Name, v, ElaCompilerError.UndefinedType, out typeModuleId);

                    //If this is a built-in type, than a variable contains data with type ID
                    //We capture this case here, thus disallowing built-in types.
                    if (sv != -1)
                        AddError(ElaCompilerError.UnableExtendBuiltin, v, v.Name);
                }
                else
                    cw.Emit(Op.Typeid, AddString(v.Name));
                
                PopVar(sca);

                if (v.HasBody)
                {
                    for (var i = 0; i < v.Constructors.Count; i++)
                    {
                        var c = v.Constructors[i];
                        CompileConstructor(v.Name, sca, c, flags, typeModuleId);
                    }
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
        private void CompileConstructor(string typeName, int sca, ElaExpression exp, ElaVariableFlags flags, int typeModuleId)
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
                            CompileConstructorConstant(typeName, n, sca, flags, typeModuleId);
                    }
                    break;
                case ElaNodeType.Juxtaposition:
                    {
                        var n = (ElaJuxtaposition)exp;

                        if (n.Target.Type == ElaNodeType.NameReference)
                        {
                            var m = (ElaNameReference)n.Target;
                            name = m.Name;

                            //If a name is uppercase or if this is an infix/postfix/prefix constructor
                            //we assume that this is a correct case and is a constructor function
                            if (m.Uppercase || (Format.IsSymbolic(m.Name) && n.Parameters.Count <= 2))
                            {
                                CompileConstructorFunction(typeName, m.Name, n, sca, flags, typeModuleId);
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
            cw.Emit(Op.Ctorid, frame.InternalConstructors.Count - 1);
            PopVar(ab);
        }

        //Compiles a simple parameterless constructor
        private void CompileConstructorConstant(string typeName, ElaNameReference exp, int sca, ElaVariableFlags flags, int typeModuleId)
        {
            frame.InternalConstructors.Add(new ConstructorData
            {
                TypeName = typeName,
                Name = exp.Name,
                Code = -1,
                TypeModuleId = typeModuleId
            });

            AddLinePragma(exp);
            cw.Emit(Op.Pushunit);
            cw.Emit(Op.Ctorid, frame.InternalConstructors.Count - 1);
            PushVar(sca);
            cw.Emit(Op.Newtype);
            var a = AddVariable(exp.Name, exp, ElaVariableFlags.TypeFun|flags, 0);
            PopVar(a);
        }

        //Compiles a type constructor
        private void CompileConstructorFunction(string typeName, string name, ElaJuxtaposition juxta, int sca, ElaVariableFlags flags, int typeModuleId)
        {
            Label funSkipLabel;
            int address;
            LabelMap newMap;
            var pars = new List<String>();
            var len = juxta.Parameters.Count;

            AddLinePragma(juxta);
            CompileFunctionProlog(name, len, juxta.Line, juxta.Column, out funSkipLabel, out address, out newMap);
            
            var sys = new int[len];
            
            for (var i = 0; i < len; i++)
            {
                var ce = juxta.Parameters[i];
                sys[i] = AddVariable();
                PopVar(sys[i]);

                if (ce.Type != ElaNodeType.NameReference || IsInvalidConstructorParameter((ElaNameReference)ce))
                    AddError(ElaCompilerError.InvalidConstructorParameter, juxta, FormatNode(ce), name);
                else
                    pars.Add(ce.GetName());
            }

            frame.InternalConstructors.Add(new ConstructorData
            {
                TypeName = typeName,
                Name = name,
                Code = -1,
                Parameters = pars,
                TypeModuleId = typeModuleId
            });
            cw.Emit(Op.Newtup, len);

            for (var i = 0; i < len; i++)
            {
                PushVar(sys[i]);
                cw.Emit(Op.Tupcons, i);
            }

            var ctid = frame.InternalConstructors.Count - 1;
            cw.Emit(Op.Ctorid, ctid);
            //Refering to captured name, need to recode its address
            PushVar((Byte.MaxValue & sca) + 1 | (sca << 8) >> 8);
            cw.Emit(Op.Newtype);

            CompileFunctionEpilog(name, len, address, funSkipLabel);
            var a = AddVariable(name, juxta, ElaVariableFlags.TypeFun|flags, len);
            PopVar(a);

            //We add special variables that can be used lately to inline this constructor call.
            var consVar = AddVariable("-$" + name, juxta, flags, len);
            var typeVar = AddVariable("--$" + name, juxta, flags, len);
            cw.Emit(Op.Ctorid, ctid);
            PopVar(consVar);
            PushVar(sca);
            PopVar(typeVar);
        }

        //Checks if a constructor parameter name is invalid
        private bool IsInvalidConstructorParameter(ElaNameReference n)
        {
            return n.Uppercase || Format.IsSymbolic(n.Name);
        }

        //Checks if an algebraic type is actually a list which implementation can be optimized.
        private bool IsList(ElaNewtype t, ElaVariableFlags flags)
        {
            if (t.Flags != ElaVariableFlags.None
                || t.Name != "List"
                || t.Extends
                )
                return false;

            return  t.Constructors.Count == 2 &&
                (t.Constructors[0].Type == ElaNodeType.NameReference && IsCons(t.Constructors[1], t))
                || (t.Constructors[1].Type == ElaNodeType.NameReference && IsCons(t.Constructors[0], t));
        }

        //Checks if a constructor is actually a list constructor which implementation can be optimized.
        private bool IsCons(ElaExpression exp, ElaNewtype t)
        {
            if (exp.Type != ElaNodeType.Juxtaposition)
                return false;

            var j = (ElaJuxtaposition)exp;

            //We check that a constructor is in the form 'a :: a'
            return j.Parameters.Count == 2 
                && j.Parameters[0].Type == ElaNodeType.NameReference
                && j.Parameters[1].Type == ElaNodeType.NameReference 
                && !Char.IsUpper(j.Parameters[0].GetName()[0])
                && !Char.IsUpper(j.Parameters[1].GetName()[0]);
        }

        //Returns a local index of a constructor
        private int ConstructorIndex(string name)
        {
            for (var i = 0; i < frame.InternalConstructors.Count; i++)
                if (name == frame.InternalConstructors[i].Name)
                    return i;

            return -1;
        }
    }
}
