using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Xsl;
using Elide.Core;
using Elide.Environment;
using Elide.Environment.Editors;
using Elide.Forms;
using Elide.HelpViewer.Images;
using Elide.CodeEditor.Infrastructure;
using System.Text;

namespace Elide.HelpViewer
{
    public sealed class HelpEditor : IEditor
    {
        private HelpView control;

        public HelpEditor()
        {
            control = new HelpView();
        }

        public void Initialize(IApp app)
        {
            App = app;
        }

        public bool TestDocumentType(FileInfo fileInfo)
        {
            return fileInfo != null && fileInfo.HasExtension("docxml");
        }

        public Document CreateDocument(string title)
        {
            var doc = new HelpDocument(title);
            return doc;
        }

        public Document OpenDocument(FileInfo fileInfo)
        {
            var title = String.Empty;
            control.SetContent(ReadFile(fileInfo, out title));
            var doc = new HelpDocument(fileInfo, title);
            return doc;
        }

        public void OpenDocument(Document doc)
        {
            var _ = default(String);
            control.SetContent(ReadFile(doc.FileInfo, out _)); 
            OpenDocument(doc.FileInfo);
        }

        public void ReloadDocument(Document doc, bool silent)
        {
            
        }

        public void Save(Document doc, FileInfo fileInfo)
        {
            
        }

        public void CloseDocument(Document doc)
        {
            
        }

        private string ReadFile(FileInfo file, out string title)
        {
            using (var sr = new StreamReader(file.OpenRead()))
            {
                var xml = new XmlDocument();
                xml.LoadXml(sr.ReadToEnd());
                title = xml.ChildNodes.OfType<XmlNode>().First(n => n.Name == "article").Attributes["title"].Value;
                var xsl = new XslCompiledTransform();
                
                using (var xslReader = new StreamReader(typeof(HelpEditor).Assembly.GetManifestResourceStream("Elide.HelpViewer.Resources.Template.xsl")))
                {
                    var tpl = xslReader.ReadToEnd();
                    xsl.Load(new XmlTextReader(new StringReader(tpl)));
                }

                var xsltArgs = new XsltArgumentList();
                xsltArgs.AddExtensionObject("http://elalang.net/elide", this);

                var sw = new StringWriter();
                xsl.Transform(xml, xsltArgs, sw);
                return sw.ToString();
            }
        }

        public string Lex(string src, string lexerKey)
        {
            var lexer = App.GetService<ICodeLexerService>().GetLexer(lexerKey);
            
            if (lexer != null)
            {
                var tokens = lexer.Parse(src);

                if (tokens != null)
                {
                    var sb = new StringBuilder(src);
                    var offset = 0;

                    tokens.ForEach(t =>
                        {
                            var span = String.Format("<span class='lexer_{0}'>", t.StyleKey);
                            var endSpan = "</span>";
                            sb.Insert(t.Position + offset, span);
                            offset += span.Length;
                            sb.Insert(t.Position + offset + t.Length, endSpan);
                            offset += endSpan.Length;
                        });

                    return sb.ToString();
                }

                return src;
            }
            else
                return src;
        }

        public Image DocumentIcon
        {
            get { return Bitmaps.Load<NS>("Icon"); }
        }

        public object Control
        {
            get { return control; }
        }

        public object Menu
        {
            get { return null; }
        }

        internal IApp App { get; private set; }
    }
}
