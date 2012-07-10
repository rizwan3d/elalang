using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part is responsible for compilation of a type class declaration
    internal sealed partial class Builder
    {
        //The main method for class compilation
        private void CompileClass(ElaTypeClass s, LabelMap map)
        {
            //We a special syntax for built-in classes and they are compiled separately
            if (s.BuiltinName != null)
                CompileBuiltinClass(s, map);
            else
            {
                //Run through members and create global class functions (Newfunc)
                for (var i = 0; i < s.Members.Count; i++)
                {
                    var m = s.Members[i];

                    //Each class function should a mask with at least one entry of a type parameter
                    if (m.Mask == 0)
                        AddError(ElaCompilerError.InvalidMemberSignature, m, m.Name);

                    var addr = AddVariable(m.Name, m, ElaVariableFlags.ClassFun, m.Arguments);
                    cw.Emit(Op.PushI4, m.Arguments);
                    cw.Emit(Op.PushI4, m.Mask);
                    cw.Emit(Op.Newfunc, AddString(m.Name));
                    PopVar(addr);
                }
            }

            //We create a special variable that will be initialized with a global unique ID of this class
            var sa = AddVariable("$$$" + s.Name, s, ElaVariableFlags.None, -1);
            cw.Emit(Op.Classid, AddString(s.Name));
            PopVar(sa);

            //Some validation
            if (frame.InternalClasses.ContainsKey(s.Name))
                AddError(ElaCompilerError.ClassAlreadyDeclared, s, s.Name);
            else
                frame.InternalClasses.Add(s.Name, new ClassData(s.Members.ToArray()));

            if (s.And != null)
                CompileClass(s.And, map);
        }

        //Built-in class compilation simply compiles an appropriate built-in and creates
        //an entry for a class as for a regular class
        private void CompileBuiltinClass(ElaTypeClass s, LabelMap map)
        {
            switch (s.BuiltinName)
            {
                case "Typeable":
                    CompileBuiltinMember(ElaBuiltinKind.Cast, s, 0, map);
                    break;
                case "Eq":
                    CompileBuiltinMember(ElaBuiltinKind.Equal, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.NotEqual, s, 1, map);
                    break;
                case "Ord":
                    CompileBuiltinMember(ElaBuiltinKind.Greater, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Lesser, s, 1, map);
                    CompileBuiltinMember(ElaBuiltinKind.GreaterEqual, s, 2, map);
                    CompileBuiltinMember(ElaBuiltinKind.LesserEqual, s, 3, map);
                    break;
                case "Additive":
                    CompileBuiltinMember(ElaBuiltinKind.Add, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Subtract, s, 1, map);
                    CompileBuiltinMember(ElaBuiltinKind.Negate, s, 2, map);
                    break;
                case "Ring":
                    CompileBuiltinMember(ElaBuiltinKind.Multiply, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Power, s, 1, map);                    
                    break;
                case "Field":
                    CompileBuiltinMember(ElaBuiltinKind.Divide, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Modulus, s, 1, map);
                    CompileBuiltinMember(ElaBuiltinKind.Remainder, s, 2, map);
                    break;
                case "Bit":
                    CompileBuiltinMember(ElaBuiltinKind.BitwiseAnd, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.BitwiseOr, s, 1, map);
                    CompileBuiltinMember(ElaBuiltinKind.BitwiseXor, s, 2, map);
                    CompileBuiltinMember(ElaBuiltinKind.BitwiseNot, s, 3, map);
                    CompileBuiltinMember(ElaBuiltinKind.ShiftLeft, s, 4, map);
                    CompileBuiltinMember(ElaBuiltinKind.ShiftRight, s, 5, map);
                    break;
                case "Enum":
                    CompileBuiltinMember(ElaBuiltinKind.Succ, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Pred, s, 1, map);
                    break;
                case "Seq":
                    CompileBuiltinMember(ElaBuiltinKind.Head, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Tail, s, 1, map);
                    CompileBuiltinMember(ElaBuiltinKind.IsNil, s, 2, map);
                    break;
                case "Ix":
                    CompileBuiltinMember(ElaBuiltinKind.Get, s, 0, map);
                    CompileBuiltinMember(ElaBuiltinKind.Length, s, 1, map);
                    break;
                case "Cat":
                    CompileBuiltinMember(ElaBuiltinKind.Concat, s, 0, map);
                    break;
                case "Show":
                    CompileBuiltinMember(ElaBuiltinKind.Showf, s, 0, map);
                    break;
                default:
                    AddError(ElaCompilerError.InvalidBuiltinClass, s, s.BuiltinName);
                    break;
            }
        }

        //Validates a built-in class definition and compile a built-in member
        private void CompileBuiltinMember(ElaBuiltinKind kind, ElaTypeClass s, int memIndex, LabelMap map)
        {
            var flags = ElaVariableFlags.Builtin | ElaVariableFlags.ClassFun;

            if (memIndex > s.Members.Count - 1)
            {
                AddError(ElaCompilerError.InvalidBuiltinClassDefinition, s, s.BuiltinName);
                return;
            }

            var m = s.Members[memIndex];
            CompileBuiltin(kind, m, map, m.Name);
            AddLinePragma(m);
            PopVar(AddVariable(m.Name, m, flags, (Int32)kind));
        }
    }
}
