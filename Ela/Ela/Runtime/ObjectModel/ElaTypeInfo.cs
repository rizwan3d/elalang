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
            var sbInst = new StringBuilder();
            sbInst.Append('(');

            if (ReflectedInstances != null)
            {
                var c = 0;
                
                foreach (var s in ReflectedInstances)
                {
                    if (c++ > 0)
                        sbInst.Append(',');

                    sbInst.Append(s);
                }

            }

            sbInst.Append(')');
            return String.Format("typeinfo {{ typeCode={0}, typeName={1}, instances={2} }}", ReflectedTypeCode, ReflectedTypeName, sbInst);
        }

        public bool HasInstance(string instance)
        {
            var ins = typeData.Instances;

            for (var i = 0; i < ins.Count; i++)
                if (ins[i] == instance)
                    return true;

            return false;
        }

        public string ReflectedTypeName
        {
            get { return typeData.TypeName; }
        }

        public int ReflectedTypeCode
        {
            get { return typeData.TypeCode; }
        }

        public IEnumerable<String> ReflectedInstances
        {
            get { return typeData.Instances; }
        }
    }
}