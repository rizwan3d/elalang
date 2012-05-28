using System;
using System.Drawing;
using System.IO;
using Elide.Core;
using Elide.EilCode.Images;
using Elide.EilCode.Lexer;
using Elide.Environment.Configuration;
using Elide.Scintilla;
using Elide.TextEditor;

namespace Elide.EilCode
{
    public sealed class EilEditor : AbstractTextEditor<EilDocument>
    {
        public EilEditor() : base("EilCode")
        {
            
        }

        protected override void InternalInitialize()
        {
            var sci = GetScintilla();

            var srv = App.GetService<IStylesConfigService>();
            srv.EnumerateStyleItems("EilCode").UpdateStyles(sci);
            UpdateTextEditorSettings();
            sci.StyleNeeded += Lex;
        }

        private void Lex(object sender, StyleNeededEventArgs e)
        {
            var lex = new EilLexer();

            foreach (var t in lex.Parse(e.Text))
                e.AddStyleItem(t.Position, t.Length, t.StyleKey);
        }

        public override bool TestDocumentType(FileInfo fileInfo)
        {
            return fileInfo != null && fileInfo.HasExtension("eil");
        }

        protected override void ConfigUpdated(Config config)
        {
            
        }

        public override Image DocumentIcon
        {
            get { return Elide.Forms.Bitmaps.Load<NS>("Icon"); }
        }
    }
}
