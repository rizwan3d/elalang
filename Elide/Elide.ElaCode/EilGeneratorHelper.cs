using System;
using Elide.Core;
using Ela.Compilation;
using Ela.Debug;

namespace Elide.ElaCode
{
    public sealed class EilGeneratorHelper
    {
        private readonly IApp app;

        public EilGeneratorHelper(IApp app)
        {
            this.app = app;
        }

        public string Generate(CodeFrame frame)
        {
            var gen = new EilGenerator(frame);
            return gen.Generate();
        }
    }
}
