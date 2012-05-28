using System;
using System.Collections.Generic;
using System.Linq;
using Ela.CodeModel;
using Ela.Compilation;
using Ela.Debug;
using Elide.CodeEditor;
using Elide.CodeEditor.Infrastructure;
using Elide.Environment;

namespace Elide.ElaCode.ObjectModel
{
    public sealed class CompiledUnit : ICompiledUnit
    {
        internal CompiledUnit(Document doc, CodeFrame codeFrame)
        {
            CodeFrame = codeFrame;
            Globals = ExtractNames(codeFrame).ToList();
            Document = doc;
            References = codeFrame.References.Where(r => !r.Key.StartsWith("$__")).Select(r => new Reference(this, r.Value)).ToList();
        }

        private IEnumerable<CodeName> ExtractNames(CodeFrame codeFrame)
        {
            if (codeFrame.Symbols == null)
                yield break;

            var dr = new DebugReader(codeFrame.Symbols);

            if (dr.EnumerateVarSyms().Count() > 0)
            {
                foreach (var v in dr.FindVarSyms(Int32.MaxValue, dr.GetScopeSymByIndex(0)))
                {
                    var flags = (ElaVariableFlags)v.Flags;

                    if (!flags.Set(ElaVariableFlags.Private))
                    {
                        var ln = dr.FindLineSym(v.Offset);
                        yield return new CodeName(v.Name, ln.Line, ln.Column);
                    }
                }
            }
            else
            {
                foreach (var v in codeFrame.GlobalScope.EnumerateNames())
                {
                    var sv = codeFrame.GlobalScope.GetVariable(v);
                    
                    if (!sv.VariableFlags.Set(ElaVariableFlags.Private))
                        yield return new CodeName(v, 0, 0);
                }
            }
        }

        internal CodeFrame CodeFrame { get; private set; }

        internal Document Document { get; private set; }

        public string Name
        {
            get { return CodeFrame.File.ShortName(); }
        }

        public IEnumerable<CodeName> Globals { get; private set; }

        public IEnumerable<IReference> References { get; private set; }
    }
}
