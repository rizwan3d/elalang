using System;

namespace Ela.Runtime
{
    public sealed class ElaTypeException : ElaRuntimeException
    {
        #region Construction
        public ElaTypeException(string tagName)
            : base("TypeLoadError", Strings.GetMessage("TypeNodeLoaded", tagName))
        {
            TypeTag = tagName;
        }
        #endregion


        #region Properties
        public string TypeTag { get; private set; }
        #endregion
    }
}
