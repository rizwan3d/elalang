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
using Ela.CodeModel;

namespace Elide.ElaCode
{
    public sealed class SymbolFinder : ISymbolFinder
    {
        private IApp app;

        public SymbolFinder(IApp app)
        {
            this.app = app;
        }

        public IEnumerable<SymbolLocation> FindSymbols(string name, bool allFiles)
        {
            if (!allFiles)
            {
                var doc = (CodeDocument)app.Document();
                return ProcessFile(name, doc);
            }
            else
            {
                return app.GetService<IDocumentService>().EnumerateDocuments()
                    .Where(d => d is CodeDocument)
                    .OfType<CodeDocument>()
                    .Select(d => ProcessFile(name, d))
                    .SelectMany(en => en);
            }

        }

        private IEnumerable<SymbolLocation> ProcessFile(string name, CodeDocument doc)
        {
            var editor = app.Editor(doc.GetType());
            var list = new List<SymbolLocation>();

            if (editor is ElaEditor)
            {
                var src = ((ITextEditor)editor).GetContent(doc);
                var p = new ElaParser();
                var res = p.Parse(src);

                if (res.Success)
                    FindName(name, doc, res.Expression, list);
            }

            return list;
        }


        private void FindName(string name, CodeDocument doc, ElaExpression expr, List<SymbolLocation> syms)
        {
            switch (expr.Type)
            {
                case ElaNodeType.AsPattern:
                    {
                        var a = (ElaAsPattern)expr;

                        if (a.Name == name)
                            syms.Add(new SymbolLocation(doc, a.Line, a.Column));

                        if (a.Pattern != null)
                            FindName(name, doc, a.Pattern, syms);
                    }
                    break;
                case ElaNodeType.Binary:
                    {
                        var b = (ElaBinary)expr;

                        if (b.Left != null)
                            FindName(name, doc, b.Left, syms);

                        if (b.Right != null)
                            FindName(name, doc, b.Right, syms);
                    }
                    break;
                case ElaNodeType.Block:
                    {
                        var b = (ElaBlock)expr;

                        foreach (var e in b.Expressions)
                            FindName(name, doc, e, syms);
                    }
                    break;
                case ElaNodeType.Builtin:
                    break;
                case ElaNodeType.Binding:
                    {
                        var b = (ElaBinding)expr;

                        if (b.Pattern == null && b.VariableName == name)
                            syms.Add(new SymbolLocation(doc, b.Line, b.Column));
                        else if (b.Pattern != null)
                            FindName(name, doc, b.Pattern, syms);

                        if (b.Where != null)
                            FindName(name, doc, b.Where, syms);

                        if (b.And != null)
                            FindName(name, doc, b.And, syms);

                        if (b.In != null)
                            FindName(name, doc, b.In, syms);

                        if (b.InitExpression != null)
                            FindName(name, doc, b.InitExpression, syms);
                    }
                    break;
                case ElaNodeType.Comprehension:
                    {
                        var c = (ElaComprehension)expr;

                        if (c.Generator != null)
                            FindName(name, doc, c.Generator, syms);

                        if (c.Initial != null)
                            FindName(name, doc, c.Initial, syms);
                    }
                    break;
                case ElaNodeType.Condition:
                    {
                        var c = (ElaCondition)expr;

                        if (c.Condition != null)
                            FindName(name, doc, c.Condition, syms);

                        if (c.True != null)
                            FindName(name, doc, c.True, syms);

                        if (c.False != null)
                            FindName(name, doc, c.False, syms);
                    }
                    break;
                case ElaNodeType.DefaultPattern:
                    break;
                case ElaNodeType.FieldDeclaration:
                    {
                        var f = (ElaFieldDeclaration)expr;

                        if (f.FieldValue != null)
                            FindName(name, doc, f.FieldValue, syms);
                    }
                    break;
                case ElaNodeType.FieldPattern:
                    {
                        var f = (ElaFieldPattern)expr;

                        if (f.Value != null)
                            FindName(name, doc, f.Value, syms);
                    }
                    break;
                case ElaNodeType.FieldReference:
                    {
                        var r = (ElaFieldReference)expr;

                        if (r.TargetObject != null)
                            FindName(name, doc, r.TargetObject, syms);
                    }
                    break;
                case ElaNodeType.FunctionCall:
                    {
                        var c = (ElaFunctionCall)expr;
                        FindName(name, doc, c.Target, syms);

                        foreach (var p in c.Parameters)
                            FindName(name, doc, p, syms);
                    }
                    break;
                case ElaNodeType.FunctionLiteral:
                    {
                        var f = (ElaFunctionLiteral)expr;

                        if (f.Body != null)
                            FindName(name, doc, f.Body, syms);
                    }
                    break;
                case ElaNodeType.Generator:
                    {
                        var g = (ElaGenerator)expr;

                        if (g.Target != null)
                            FindName(name, doc, g.Target, syms);

                        if (g.Pattern != null)
                            FindName(name, doc, g.Pattern, syms);

                        if (g.Guard != null)
                            FindName(name, doc, g.Guard, syms);

                        if (g.Body != null)
                            FindName(name, doc, g.Body, syms);
                    }
                    break;
                case ElaNodeType.HeadTailPattern:
                    {
                        var h = (ElaHeadTailPattern)expr;

                        if (h.Patterns != null)
                            foreach (var p in h.Patterns)
                                FindName(name, doc, p, syms);
                    }
                    break;
                case ElaNodeType.ImportedVariable:
                    {
                        var v = (ElaImportedVariable)expr;

                        if (v.LocalName == name || v.Name == name)
                            syms.Add(new SymbolLocation(doc, v.Line, v.Column));
                    }
                    break;
                case ElaNodeType.Is:
                    {
                        var i = (ElaIs)expr;

                        if (i.Pattern != null)
                            FindName(name, doc, i.Pattern, syms);

                        if (i.Expression != null)
                            FindName(name, doc, i.Expression, syms);
                    }
                    break;
                case ElaNodeType.IsPattern:
                    break;
                case ElaNodeType.LazyLiteral:
                    {
                        var l = (ElaLazyLiteral)expr;

                        if (l.Body != null)
                            FindName(name, doc, l.Body, syms);
                    }
                    break;
                case ElaNodeType.ListLiteral:
                    {
                        var l = (ElaListLiteral)expr;

                        if (l.Values != null)
                            foreach (var v in l.Values)
                                FindName(name, doc, v, syms);
                    }
                    break;
                case ElaNodeType.LiteralPattern:
                    break;
                case ElaNodeType.Match:
                    {
                        var m = (ElaMatch)expr;

                        if (m.Expression != null)
                            FindName(name, doc, m.Expression, syms);

                        if (m.Entries != null)
                            foreach (var e in m.Entries)
                                FindName(name, doc, e, syms);
                    }
                    break;
                case ElaNodeType.MatchEntry:
                    {
                        var e = (ElaMatchEntry)expr;

                        if (e.Pattern != null)
                            FindName(name, doc, e.Pattern, syms);

                        if (e.Guard != null)
                            FindName(name, doc, e.Guard, syms);

                        if (e.Where != null)
                            FindName(name, doc, e.Where, syms);

                        if (e.Expression != null)
                            FindName(name, doc, e.Expression, syms);
                    }
                    break;
                case ElaNodeType.ModuleInclude:
                    {
                        var m = (ElaModuleInclude)expr;

                        if (m.Alias == name)
                            syms.Add(new SymbolLocation(doc, m.Line, m.Column));

                        if (m.HasImportList)
                            foreach (var i in m.ImportList)
                                FindName(name, doc, i, syms);
                    }
                    break;
                case ElaNodeType.NilPattern:
                    break;
                case ElaNodeType.OtherwiseGuard:
                    break;
                case ElaNodeType.PatternGroup:
                    {
                        var g = (ElaPatternGroup)expr;

                        if (g.Patterns != null)
                            foreach (var p in g.Patterns)
                                FindName(name, doc, p, syms);
                    }
                    break;
                case ElaNodeType.Placeholder:
                    break;
                case ElaNodeType.Primitive:
                    break;
                case ElaNodeType.Raise:
                    {
                        var r = (ElaRaise)expr;

                        if (r.Expression != null)
                            FindName(name, doc, r.Expression, syms);
                    }
                    break;
                case ElaNodeType.Range:
                    {
                        var r = (ElaRange)expr;

                        if (r.Initial != null)
                            FindName(name, doc, r.Initial, syms);

                        if (r.First != null)
                            FindName(name, doc, r.First, syms);

                        if (r.Second != null)
                            FindName(name, doc, r.Second, syms);

                        if (r.Last != null)
                            FindName(name, doc, r.Last, syms);
                    }
                    break;
                case ElaNodeType.RecordLiteral:
                    {
                        var r = (ElaRecordLiteral)expr;

                        if (r.Fields != null)
                            foreach (var f in r.Fields)
                                FindName(name, doc, f, syms);
                    }
                    break;
                case ElaNodeType.RecordPattern:
                    {
                        var r = (ElaRecordPattern)expr;

                        if (r.Fields != null)
                            foreach (var f in r.Fields)
                                FindName(name, doc, f, syms);
                    }
                    break;
                case ElaNodeType.Try:
                    {
                        var t = (ElaTry)expr;

                        if (t.Expression != null)
                            FindName(name, doc, t.Expression, syms);

                        if (t.Entries != null)
                            foreach (var e in t.Entries)
                                FindName(name, doc, e, syms);
                    }
                    break;
                case ElaNodeType.TupleLiteral:
                    {
                        var t = (ElaTupleLiteral)expr;

                        if (t.Parameters != null)
                            foreach (var p in t.Parameters)
                                FindName(name, doc, p, syms);
                    }
                    break;
                case ElaNodeType.TuplePattern:
                    {
                        var t = (ElaTuplePattern)expr;

                        if (t.Patterns != null)
                            foreach (var p in t.Patterns)
                                FindName(name, doc, p, syms);
                    }
                    break;
                case ElaNodeType.UnitLiteral:
                    break;
                case ElaNodeType.UnitPattern:
                    break;
                case ElaNodeType.VariablePattern:
                    {
                        var v = (ElaVariablePattern)expr;

                        if (v.Name == name)
                            syms.Add(new SymbolLocation(doc, v.Line, v.Column));
                    }
                    break;
                case ElaNodeType.VariableReference:
                    {
                        var r = (ElaVariableReference)expr;

                        if (r.VariableName == name)
                            syms.Add(new SymbolLocation(doc, r.Line, r.Column));
                    }
                    break;
                case ElaNodeType.VariantLiteral:
                    {
                        var v = (ElaVariantLiteral)expr;

                        if (v.Tag == name)
                            syms.Add(new SymbolLocation(doc, v.Line, v.Column));
                    }
                    break;
                case ElaNodeType.VariantPattern:
                    {
                        var v = (ElaVariantPattern)expr;

                        if (v.Tag == name)
                            syms.Add(new SymbolLocation(doc, v.Line, v.Column));

                        if (v.Pattern != null)
                            FindName(name, doc, v.Pattern, syms);
                    }
                    break;
            }
        }
    }
}
