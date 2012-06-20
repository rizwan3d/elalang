﻿using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
    public sealed class ExportVars
    {
        private readonly Dictionary<String,ExportVarData> variables;
        
        public ExportVars()
        {
            variables = new Dictionary<String,ExportVarData>();
        }
        
        public void AddVariable(string name, ElaBuiltinKind kind, ElaVariableFlags flags, int moduleHandle, int address)
        {
            variables.Remove(name);
            variables.Add(name, new ExportVarData(kind, flags, moduleHandle, address));
        }

        public bool FindVariable(string name, out ExportVarData data)
        {
            return variables.TryGetValue(name, out data);
        }

        public bool HasVariable(string name)
        {
            return variables.ContainsKey(name);
        }
    }
}
