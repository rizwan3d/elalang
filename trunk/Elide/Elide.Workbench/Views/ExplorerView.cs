using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Elide.Core;
using Elide.Workbench;
using Elide.Forms;
using System.Drawing;
using Elide.Workbench.Images;
using Elide.Environment;
using Elide.Environment.Editors;
using System.Collections.Generic;

namespace Elide.Workbench.Views
{
	public sealed class ExplorerView : Elide.Environment.Views.AbstractView
	{
		private ExplorerControl control;
        private LazyTreeView treeView;
        private ImageList imageList;
        private Dictionary<String,String> extMap;

        public ExplorerView()
		{
            extMap = new Dictionary<String,String>();
            control = new ExplorerControl();
            treeView = control.TreeView;

            imageList = new ImageList();
            imageList.ImageSize = new Size(16, 16);
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.TransparentColor = Color.Magenta;
            treeView.ImageList = imageList;

            imageList.Images.Add("Drive", Bitmaps.Load<NS>("Drive"));
            imageList.Images.Add("Folder", Bitmaps.Load<NS>("Folder"));
            imageList.Images.Add("File", Bitmaps.Load<NS>("File"));
		}
		
		public override void  Initialize(IApp app)
        {
 	        base.Initialize(app);

            treeView.NodesNeeded += (o,e) => NodesNeeded(e.Node);
			treeView.NodeMouseDoubleClick += (o,e) =>
			{
                if (e.Node.Tag is FileInfo)
                    App.GetService<IFileService>().OpenFile((FileInfo)e.Node.Tag);
			};
		}

        public override void Activate()
        {
            if (treeView.Nodes.Count == 0)
                InitializeTreeView();

            treeView.Select();
        }

        private void InitializeTreeView()
        {
            var svc = App.GetService<IEditorService>();
            svc.EnumerateInfos("editors").OfType<EditorInfo>().ForEach(e =>
            {
                extMap.Add(e.FileExtension.ToLower(), e.Key);
                treeView.ImageList.Images.Add(e.Key, e.Instance.DocumentIcon);
            });
			
			try
			{
				treeView.BeginUpdate();

                foreach (var d in DriveInfo.GetDrives())
                {
                    var n = CreateNode(d.Name, "Drive", d);
                    treeView.AddLazyNode(n);
                }

                treeView.ContextMenuStrip = BuildMenu();
			}
			finally
			{
				treeView.EndUpdate();
			}
        }

		private void NodesNeeded(TreeNode node)
		{
			if (node.Tag is DriveInfo)
			{
				var d = (DriveInfo)node.Tag;
				ProcessDirectory(d.RootDirectory, node);
				
			}
			else if (node.Tag is DirectoryInfo)
			{
				var d = (DirectoryInfo)node.Tag;
				ProcessDirectory(d, node);
			}
		}
        
		private void ProcessDirectory(DirectoryInfo dir, TreeNode node)
		{
			try
			{
				treeView.BeginUpdate();

				foreach (var e in dir.GetFileSystemInfos())
				{
					if ((e.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden || control.ShowHiddenFolders)
                    {
                        var isFolder = e is DirectoryInfo;
                        var img = "Folder";

                        if (!isFolder)
                        {
                            var fi = (FileInfo)e;

                            if (!extMap.TryGetValue(fi.Extension.ToLower(), out img))
                                img = "File";
                        }

                        var n = CreateNode(e.Name, img, e);

                        if (isFolder)
                            treeView.AddLazyNode(node, n);
                        else
                        {
                            var fi = (FileInfo)e;
            
                            if (control.ShowAllTypes || control.HasDocumentType(fi.Extension))
                                node.Nodes.Add(n);
                        }
                    }
				}
			}
			finally
			{
				treeView.EndUpdate();
			}
		}

        public ContextMenuStrip BuildMenu()
        {
            var builder = App.GetService<IMenuService>().CreateMenuBuilder<ContextMenuStrip>();
            return builder
                .Item("Refresh", () => control.Refresh(false))
                .Separator()
                .Item("Show All Files", null, control.ToggleShowAllTypes, null, () => control.ShowAllTypes)
                .Item("Show Hidden Folders", null, control.ToggleShowHiddenFolders, null, () => control.ShowHiddenFolders)
                .Separator()
                .Items(BuildEditorList)
                .Separator()
                .Item("Open File", null, OpenFile, () => treeView.SelectedNode != null && treeView.SelectedNode.Tag is FileInfo)
                .Finish();
        }

        private void OpenFile()
        {
            var fi = (FileInfo)treeView.SelectedNode.Tag;
            App.GetService<IFileService>().OpenFile(fi);
        }

        private void BuildEditorList(IMenuBuilder<ContextMenuStrip> builder)
        {
            App.GetService<IEditorService>()
                .EnumerateInfos("editors")
                .OfType<EditorInfo>()
                .ForEach(e => builder.Item("Show: " + e.DisplayName, null, 
                    () => {
                        if (control.HasDocumentType(e.FileExtension))
                            control.RemoveDocumentType(e.FileExtension);
                        else
                            control.AddDocumentType(e.FileExtension);
                    },
                    () => !control.ShowAllTypes, () => control.HasDocumentType(e.FileExtension)));
        }

        private TreeNode CreateNode(string text, string image, object tag)
        {
            return new TreeNode(text) { ImageKey = image, SelectedImageKey = image, Tag = tag };
        }

        public override object Control
        {
            get { return control; }
        }
	}
}
