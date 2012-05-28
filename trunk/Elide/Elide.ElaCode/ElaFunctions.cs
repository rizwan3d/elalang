﻿using System;
using System.IO;
using Ela.Linking;
using Elide.CodeEditor;
using Elide.CodeEditor.Infrastructure;
using Elide.Core;
using Elide.ElaCode.ObjectModel;
using Elide.Environment;
using Elide.Environment.Editors;
using Elide.Scintilla;
using Elide.TextEditor;

namespace Elide.ElaCode
{
    internal sealed class ElaFunctions
    {
        private readonly IApp app;
        private readonly ScintillaControl sci;

        internal ElaFunctions(IApp app, ScintillaControl sci)
        {
            this.app = app;
            this.sci = sci;
        }

        public void Run()
        {
            var asm = app.GetService<ICodeBuilderService>().
                RunBuilder<CompiledAssembly>(sci.Text, app.Document(), BuildOptions.Output | BuildOptions.ErrorList);

            if (asm != null)
                app.GetService<ICodeRunnerService>().
                    RunCode(asm, ExecOptions.Annotation | ExecOptions.PrintResult | ExecOptions.Console | ExecOptions.ShowOutput);
        }

        public void RunSelected()
        {
            Func<String,CompiledAssembly> fun = s => app.GetService<ICodeBuilderService>().
                 RunBuilder<CompiledAssembly>(s, app.Document(), BuildOptions.Output | BuildOptions.TipError, ElaCodeBuilder.NoDebug, ElaCodeBuilder.NoWarnings);
            var sel = sci.GetSelection().Text;
            var src = sci.Text + "\r\nlet _=()\r\n" + sel;            
            var asm = fun(src);

            if (asm == null)
                asm = fun(sel);

            if (asm != null)
            {
                var svc = app.GetService<ICodeRunnerService>();

                if (!svc.RunCode(asm, ExecOptions.PrintResult | ExecOptions.TipResult | ExecOptions.TipError | ExecOptions.LimitTime))
                {
                    asm = fun(sel);

                    if (asm != null)
                        svc.RunCode(asm, ExecOptions.PrintResult | ExecOptions.TipResult | ExecOptions.TipError);
                }
            }
        }

        public void MakeObjectFile()
        {
            var asm = app.GetService<ICodeBuilderService>().
               RunBuilder<CompiledAssembly>(sci.Text, app.Document(), BuildOptions.Output | BuildOptions.ErrorList, ElaCodeBuilder.ForceRecompile, ElaCodeBuilder.NoDebug);

            if (asm != null)
            {
                var fi = app.GetService<IDialogService>().ShowSaveDialog(app.Document().Title.Replace(".ela", String.Empty) + ".elaobj");

                if (fi != null)
                {
                    var wr = new ObjectFileWriter(fi);
                    wr.Write(asm.Assembly.GetRootModule());
                }
            }
        }

        public void GenerateEil()
        {
            var asm = app.GetService<ICodeBuilderService>().
                RunBuilder<CompiledAssembly>(sci.Text, app.Document(), BuildOptions.Output | BuildOptions.ErrorList, ElaCodeBuilder.ForceRecompile);

            if (asm != null)
            {
                var frame = asm.Assembly.GetRootModule();
                var gen = new EilGeneratorHelper(app);
                var src = gen.Generate(frame);

                var editor = (EditorInfo)app.GetService<IEditorService>().GetInfo("editors", "EilCode");

                var fi = new FileInfo(app.Document().Title);
                var doc = editor.Instance.CreateDocument(fi.Name.Replace(fi.Extension, editor.FileExtension));
                app.GetService<IDocumentService>().AddDocument(doc);
                ((ITextEditor)editor.Instance).SetContent(doc, src);
            }
        }

        public void FindSymbol()
        {
            app.GetService<ISymbolSearchService>().SearchSymbol(new SymbolFinder(app));
        }

        public void Autocomplete()
        {
            Autocomplete(sci.CurrentPosition);
        }
        
        public void Autocomplete(int position)
        {
            var am = new AutocompleteManager(app, sci);
            am.DoComplete(position, app.Document() as CodeDocument);
        }
    }
}
