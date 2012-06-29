using System;
using System.Collections.Generic;
using System.Text;
using Ela.Linking;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaTypeInfo : ElaObject
	{
        private readonly TypeData typeData;

        internal ElaTypeInfo(TypeData typeData) : base(ElaTypeCode.TypeInfo)
		{
            this.typeData = typeData;
		}

        public override string ToString(string format, IFormatProvider formatProvider)
        {
            return String.Format("typeinfo {{ typeCode={0}, typeName={1} }}", ReflectedTypeCode, ReflectedTypeName);
        }

        public string ReflectedTypeName
        {
            get { return typeData.TypeName; }
        }

        public int ReflectedTypeCode
        {
            get { return typeData.TypeCode; }
        }
    }
}