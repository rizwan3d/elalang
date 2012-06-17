using System;

namespace Ela.Compilation
{
    internal sealed class InstanceData
    {
        public readonly string Type;
        public readonly string Class;
        public readonly int TypeModuleId;
        public readonly int ClassModuleId;
        public readonly int Line;
        public readonly int Column;

        public InstanceData(string type, string @class, int typeModuleId, int classModuleId, int line, int column)
        {
            Type = type;
            Class = @class;
            TypeModuleId = typeModuleId;
            ClassModuleId = classModuleId;
            Line = line;
            Column = column;
        }
    }
}
