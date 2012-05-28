using System;
using System.Collections.Generic;
using System.Linq;
using Ela.Compilation;
using Ela.Debug;
using Ela.Parsing;
using Elide.CodeEditor;
using Elide.Core;
using Elide.Environment;
using Elide.TextEditor;

namespace Elide.ElaCode
{
    public sealed class SymbolFinder : ISymbolFinder
    {
        private IApp app;

        public SymbolFinder(IApp app)
        {
            this.app = app;
        }

        public IEnumerable<SymbolLocation> FindSymbols(string name, bool onlyGlobals, bool allFiles)
        {
            if (!allFiles)
            {
                var doc = (CodeDocument)app.Document();
                return ProcessFile(name, doc, onlyGlobals);
            }
            else
            {
                return app.GetService<IDocumentService>().EnumerateDocuments()
                    .Where(d => d is CodeDocument)
                    .OfType<CodeDocument>()
                    .Select(d => ProcessFile(name, d, onlyGlobals))
                    .SelectMany(en => en);
            }

        }

        private IEnumerable<SymbolLocation> ProcessFile(string name, CodeDocument doc, bool onlyGlobals)
        {
            var editor = app.Editor(doc.GetType());

            if (editor is ElaEditor)
            {
                var src = ((ITextEditor)editor).GetContent(doc);
                var p = new ElaParser();
                var res = p.Parse(src);

                if (res.Success)
                {
                    var c = new ElaCompiler();
                    var comRes = c.Compile(res.Expression, new CompilerOptions { Prelude = "Prelude", GenerateDebugInfo = true }, new ExportVars());
                    var frame = comRes.CodeFrame;

                    if (frame != null && frame.Symbols != null)
                    {
                        var dr = new DebugReader(frame.Symbols);
                        return ProcessDebugInfo(name, doc, dr, onlyGlobals);
                    }
                }
            }

            return new List<SymbolLocation>();
        }


        private IEnumerable<SymbolLocation> ProcessDebugInfo(string name, CodeDocument doc, DebugReader dr, bool globals)
        {
            foreach (var vs in dr.EnumerateVarSyms())
            {
                if (vs.Name == name && (vs.Scope == 0 || !globals))
                {
                    var ls = dr.FindLineSym(vs.Offset);

                    if (ls != null)
                        yield return new SymbolLocation(doc, ls.Line - 1, ls.Column - 1);
                }
            }
        }
    }
}
