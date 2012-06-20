using System;
using Ela.CodeModel;

namespace Ela.Compilation
{
    //This part contains compilation logic for built-in and user defined types.
    internal sealed partial class Builder
    {
        //A name of a currently processed type (used by CompileFunction method)
        private string typeName;
        
        //Main method for type compilation
        private void CompileType(ElaNewtype v, LabelMap map, Hints hints)
        {
            //We need to obtain type code for a type
            var tc = -1;

            //A body may be null only if this is a built-in type
            if (v.Body == null)
            {
                tc = (Int32)TypeCodeFormat.GetTypeCode(v.Name);

                //OK, type is no built-in, this cannot work
                if (tc == 0)
                    AddError(ElaCompilerError.OnlyBuiltinTypeNoDefinition, v, v.Name);

                //We add a special variable that contains a global type ID
                var sca = AddVariable("$$" + v.Name, v, ElaVariableFlags.None, -1);
                cw.Emit(Op.PushI4, tc);
                PopVar(sca);
            }

            //Type is already declared within the same module (types from different
            //modules can shadow each, so this is perfectly OK).
            if (frame.InternalTypes.ContainsKey(v.Name))
                AddError(ElaCompilerError.TypeAlreadyDeclared, v, v.Name);
            else
            {
                frame.InternalTypes.Add(v.Name, tc);

                //A regular type definition with body
                if (v.Body != null)
                {
                    //A type definition is basically a function so we need to compile
                    //it as a function. However a last op code in this function should
                    //be Newtype (that is why we need Newtype flag).
                    typeName = v.Name; //This is used by CompileFunction method
                    CompileFunction(v, FunFlag.Newtype);
                    var addr = AddVariable(v.Name, v, ElaVariableFlags.TypeFun, -1);
                    AddLinePragma(v);
                    PopVar(addr);

                    //Add a special variable with global type ID which will be calculated
                    //at run-time
                    var sa = AddVariable("$$" + v.Name, v, ElaVariableFlags.None, -1);
                    cw.Emit(Op.Typeid, AddString(v.Name));
                    PopVar(sa);
                }
            }
        }
    }
}
