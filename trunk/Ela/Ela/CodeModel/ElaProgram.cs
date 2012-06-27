using System;

namespace Ela.CodeModel
{
    public sealed class ElaProgram
    {
        public ElaProgram()
        {
            TopLevel = new ElaEquationSet();
        }

        public ElaEquationSet TopLevel { get; private set; }

        public ElaModuleInclude Includes { get; internal set; }

        public ElaTypeClass Classes { get; internal set; }

        public ElaNewtype Types { get; internal set; }

        public ElaClassInstance Instances { get; internal set; }
    }
}
