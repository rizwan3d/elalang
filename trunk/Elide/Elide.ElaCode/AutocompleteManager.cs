using System;
using System.Collections.Generic;
using System.Linq;
using Ela.CodeModel;
using Ela.Compilation;
using Ela.Debug;
using Elide.CodeEditor;
using Elide.Core;
using Elide.Scintilla;
using Elide.ElaCode.ObjectModel;

namespace Elide.ElaCode
{
    public sealed class AutocompleteManager
    {
        private readonly ScintillaControl sci;
        private readonly IApp app;

        public AutocompleteManager(IApp app, ScintillaControl sci)
        {
            this.app = app;
            this.sci = sci;
        }

        private bool TestIfComments(int pos, bool checkStr)
        {
            var st = sci.GetStyleAt(pos);

            //exclude autocomplete in comments and strings
            var b = st == TextStyle.MultilineStyle1 || st == TextStyle.MultilineStyle2
                || st == TextStyle.Style6 || st == TextStyle.Style7;

            if (st == TextStyle.None && checkStr)
            {
                var lnn = sci.GetLineFromPosition(pos);
                var ln = sci.GetLine(lnn);
                var col = sci.GetColumnFromPosition(pos);

                for (var i = col; i > -1; i--)
                    if (sci.CharAt(sci.GetPositionByColumn(lnn, i)) == '"')
                        return true;
            }

            return b;
        }

        public void DoComplete(int pos, CodeDocument doc)
        {
            if (doc == null)
                return;

            if (TestIfComments(pos, true) || TestIfComments(pos - 1, false))
                return;

            var names = default(List<AutocompleteSymbol>);

            if (doc.Unit != null)
            {
                var frame = ((CompiledUnit)doc.Unit).CodeFrame;
                var dr = new DebugReader(frame.Symbols);
                var col = sci.GetColumnFromPosition(sci.CurrentPosition);
                var ln = dr.FindClosestLineSym(sci.CurrentLine + 1, col + 1);

                var scope = col == 0 || col == 1 ? dr.GetScopeSymByIndex(0) :
                    dr.FindScopeSym(sci.CurrentLine + 1, col + 1);

                if (scope == null)
                    scope = dr.GetScopeSymByIndex(0);

                if (scope != null)
                {
                    var vars = LookVars(dr, ln != null ? ln.Offset : scope.EndOffset, scope.Index);
                    names = vars
                        .Where(v => v.Name[0] != '$')
                        .Select(v => 
                            {
                                var f = (ElaVariableFlags)v.Flags;
                                return new AutocompleteSymbol(v.Name,
                                    f.Set(ElaVariableFlags.Module) ? AutocompleteSymbolType.Module :
                                    f.Set(ElaVariableFlags.TypeFun) ? AutocompleteSymbolType.Type :
                                    f.Set(ElaVariableFlags.ClassFun) ? AutocompleteSymbolType.Member :
                                        AutocompleteSymbolType.Variable);
                                
                            })
                        .ToList();
                }
            }

            var line = sci.GetLine(sci.CurrentLine).Text.Trim('\r', '\n', '\0');
            var tl = line.Trim();
            var keywords = new List<AutocompleteSymbol>();

            keywords.Add(Snippet("if"));

            if (tl.Length == 0)
            {
                keywords.Add(Snippet("open"));
                keywords.Add(Snippet("let"));

                if (line.Length > 0)
                {
                    keywords.Add(Snippet("et"));
                    keywords.Add(Snippet("where"));
                }
            }
            else if (tl.EndsWith("="))
                keywords.Add(Snippet("let"));
            else if (tl.EndsWith("let"))
            {
                keywords.Add(Snippet("private"));
                keywords.Add(Snippet("inline"));
            }

            if (names != null)
                keywords.AddRange(names);

            app.GetService<IAutocompleteService>().ShowAutocomplete(keywords);
        }

        private AutocompleteSymbol Snippet(string text)
        {
            return new AutocompleteSymbol(text, AutocompleteSymbolType.Snippet);
        }
        
        private IEnumerable<VarSym> LookVars(DebugReader dr, int offset, int scopeIndex)
        {
            var scope = dr.GetScopeSymByIndex(scopeIndex);
            var list = new List<VarSym>();

            foreach (var vs in dr.FindVarSyms(offset, scope))
                list.Add(vs);

            if (scope.Index != 0 && scope.ParentIndex != scope.Index)
                list.AddRange(LookVars(dr, offset, scope.ParentIndex));

            return list;
        }
    }
}
