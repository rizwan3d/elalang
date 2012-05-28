using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Elide.Forms;
using Elide.CodeWorkbench.Images;
using Elide.Scintilla;
using Elide.Core;
using System.IO;
using Elide.CodeEditor;
using Elide.CodeEditor.Infrastructure;
using Elide.Environment;
using Elide.TextEditor;

namespace Elide.CodeWorkbench.Views
{
    public partial class OutlineControl : UserControl
    {
        public OutlineControl()
        {
            InitializeComponent();
            treeView.Font = Fonts.ControlText;
            var imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(16, 16);
            imageList.TransparentColor = Color.Magenta;

            imageList.Images.Add("Folder", Bitmaps.Load<NS>("Folder"));
            imageList.Images.Add("References", Bitmaps.Load<NS>("References"));
            imageList.Images.Add("Reference", Bitmaps.Load<NS>("Reference"));
            imageList.Images.Add("Variable", Bitmaps.Load<NS>("Variable"));
            imageList.Images.Add("Module", Bitmaps.Load<NS>("Module"));
            treeView.ImageList = imageList;
        }

        public void Clear()
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();            
            treeView.EndUpdate();
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is CodeName)
            {
                var doc = (CodeDocument)e.Node.Parent.Parent.Tag;
                App.GetService<IDocumentService>().SetActiveDocument(doc);

                var sym = (CodeName)e.Node.Tag;
                App.GetService<IDocumentNavigatorService>().Navigate(doc, sym.Line - 1, sym.Column - 1, 0, true);
            }
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;

            if (node.Text == "Bindings")
            {
                var doc = (CodeDocument)node.Parent.Tag;

                treeView.BeginUpdate();
                node.Nodes.Clear();

                if (doc.Unit != null)
                {
                    foreach (var v in doc.Unit.Globals)
                    {
                        var tn = new TreeNode(v.Name) { ImageKey = "Variable", SelectedImageKey = "Variable" };
                        tn.Tag = v;
                        node.Nodes.Add(tn);
                    }
                }

                treeView.EndUpdate();
            }
            else if (node.Text == "References")
            {
                var doc = (CodeDocument)node.Parent.Tag;

                treeView.BeginUpdate();
                node.Nodes.Clear();

                if (doc.Unit != null)
                {
                    foreach (var mr in doc.Unit.References)
                    {
                        var tn = new TreeNode(mr.ToString()) { ImageKey = "Reference", SelectedImageKey = "Reference" };
                        tn.Tag = mr;
                        tn.Nodes.Add(new TreeNode());
                        node.Nodes.Add(tn);
                    }
                }

                treeView.EndUpdate();
            }

            else if (node.Tag is IReference)
            {
                treeView.BeginUpdate();
                node.Nodes.Clear();

                var unit = App.GetService<IReferenceResolverService>().Resolve((IReference)node.Tag);
                
                if (unit != null)
                {
                    foreach (var v in unit.Globals)
                    {
                        var tn = new TreeNode(v.Name) { ImageKey = "Variable", SelectedImageKey = "Variable" };
                        node.Nodes.Add(tn);
                    }
                }

                treeView.EndUpdate();
            }
        }

        public TreeNode AddDocumentNode(CodeDocument doc)
        {
            var tn = new TreeNode(doc.FileInfo != null ? doc.FileInfo.ShortName() : new FileInfo(doc.Title).ShortName());
            tn.ImageKey = "Module";
            tn.SelectedImageKey = "Module";
            tn.Tag = doc;
            AddDocumentNodes(tn);
            treeView.Nodes.Add(tn);
            return tn;
        }

        public void RemoveDocumentNode(CodeDocument doc)
        {
            var node = treeView.Nodes.OfType<TreeNode>().FirstOrDefault(tn => tn.Tag == doc);

            if (node != null)
            {
                node.Tag = null;
                treeView.Nodes.Remove(node);
            }
        }

        public void CollapseNode(CodeDocument doc)
        {
            var node = treeView.Nodes.OfType<TreeNode>().FirstOrDefault(tn => tn.Tag == doc);

            if (node != null)
            {
                if (node.IsExpanded && (node.FirstNode.IsExpanded || node.Nodes[1].IsExpanded || 
                    node.FirstNode.Nodes.Count == 0 || node.Nodes[1].Nodes.Count == 0))
                {
                    node.Nodes.Clear();
                    AddDocumentNodes(node);
                }
            }
        }

        private void AddDocumentNodes(TreeNode tn)
        {
            var refs = new TreeNode("References");
            refs.ImageKey = "References"; 
            refs.SelectedImageKey = "References";
            refs.Nodes.Add(new TreeNode());
            tn.Nodes.Add(refs);

            var binds = new TreeNode("Bindings");
            binds.ImageKey = "Folder";
            binds.SelectedImageKey = "Folder";
            binds.Nodes.Add(new TreeNode());
            tn.Nodes.Add(binds);
        }

        public TreeView TreeView
        {
            get { return treeView; }
        }
        
        internal IApp App { get; set; }
    }
}
