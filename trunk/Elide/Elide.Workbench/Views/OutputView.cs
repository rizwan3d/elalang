using System;
using System.Linq;
using System.Windows.Forms;
using Elide.Core;
using Elide.Environment;
using Elide.Environment.Configuration;
using Elide.Environment.Views;
using Elide.Scintilla;
using Elide.Scintilla.ObjectModel;

namespace Elide.Workbench.Views
{
    public sealed class OutputView : AbstractView
    {
        private OutputControl control;
        private ScintillaControl sci;
        
        public OutputView()
        {
            this.control = new OutputControl();
            this.sci = control.Scintilla;
            this.sci.StyleNeeded += Lex;
        }

        public override void Initialize(IApp app)
        {
            base.Initialize(app);
            
            var srv = App.GetService<IStylesConfigService>();
            srv.EnumerateStyleItems("Output").UpdateStyles(sci);
            App.GetService<IConfigService>().ConfigUpdated += ConfigUpdated;

            var builder = App.GetService<IMenuService>().CreateMenuBuilder<ContextMenuStrip>();
            var menu = builder
                .Item("Copy", sci.Copy, sci.HasSelections)
                .Item("Select All", sci.SelectAll, () => sci.GetTextLength() > 0)
                .Separator()
                .Item("Clear", Clear, () => sci.GetTextLength() > 0)
                .Separator()
                .Item("Word Wrap", null, sci.ToggleWrapMode, null, () => sci.WordWrapMode != WordWrapMode.None)
                .Finish();
            sci.ContextMenuStrip = menu;
        }

        public override void Activate()
        {
            sci.Select();
        }

        private void ConfigUpdated(object sender, ConfigEventArg e)
        {
            if (e.Config is StylesConfig)
            {
                var c = (StylesConfig)e.Config;
                c.Styles["Output"].UpdateStyles(sci);
            }
        }

        public void WriteString(string text, params object[] args)
        {
            if (args != null && args.Length > 0)
                sci.AppendText(String.Format(text, args), true);
            else
                sci.AppendText(text, true);

            OnContentChanged(new ViewEventArgs(true, null));
        }
        
        public void Clear()
        {
            sci.SetText(String.Empty);
            OnContentChanged(ViewEventArgs.None);
        }

        public override object Control
        {
            get { return control; }
        }

        private void Lex(object sender, StyleNeededEventArgs e)
        {
            var lexer = new OutputLexer();
            lexer.Parse(e.Text).ToList().ForEach(t => e.AddStyleItem(t.Position, t.Length, t.StyleKey));
        }
    }
}
