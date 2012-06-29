using System;

namespace Ela.Linking
{
    internal sealed class TypeData
    {
        public TypeData(ElaTypeCode typeCode) : this((Int32)typeCode, TypeCodeFormat.GetShortForm(typeCode))
        {
            
        }

        public TypeData(int typeCode, string typeName)
        {
            TypeName = typeName;
            TypeCode = typeCode;
        }

        public int TypeCode { get; private set; }

        public string TypeName { get; private set; }
    }
}
