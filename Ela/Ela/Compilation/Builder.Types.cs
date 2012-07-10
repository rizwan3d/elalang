using System;
using Ela.CodeModel;

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
            var sca = AddVariable("$$" + v.Name, v, ElaVariableFlags.None, -1);
                
            //A body may be null only if this is a built-in type
            if (!v.HasBody)
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
            if (frame.InternalTypes.ContainsKey(v.Name))
                AddError(ElaCompilerError.TypeAlreadyDeclared, v, v.Name);
            else
            {
                frame.InternalTypes.Add(v.Name, tc);
                AddLinePragma(v);

                //Add a type var for a non built-in type
                if (v.HasBody)
                {
                    //Add a special variable with global type ID which will be calculated at run-time
                    cw.Emit(Op.Typeid, AddString(v.Name));
                    PopVar(sca);
                }
            }

            if (v.And != null)
                CompileType(v.And, map);
        }
    }
}
