using System;

namespace Ela.Runtime
{
    internal struct ParamInfo : IEquatable<ParamInfo>
    {
        #region Construction
        internal static readonly ParamInfo Any = new ParamInfo(0, null);

        internal readonly int Index;
        internal readonly string Tag;
        
        
        internal ParamInfo(int index, string tag)
        {
            Index = index;
            Tag = tag;
        }
        #endregion


        #region Methods
        public static bool Equals(ParamInfo left, ParamInfo right)
        {
            return left.Index == right.Index && left.Tag == right.Tag;
        }

        public override string ToString()
        {
            return Index + ";" + Tag;
        }
        
        public override bool Equals(object obj)
        {
            return obj is ParamInfo && Equals(this, (ParamInfo)obj);
        }

        public bool Equals(ParamInfo val)
        {
            return Index == val.Index && Tag == val.Tag;
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
        #endregion
    }
}
