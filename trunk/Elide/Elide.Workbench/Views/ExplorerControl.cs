using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Elide.Forms.State;

namespace Elide.Workbench.Views
{
    public partial class ExplorerControl : StateUserControl
    {
        public ExplorerControl()
        {
            InitializeComponent();
            ShowAllTypes = true;
        }
        
        public void Refresh(bool force)
        {
            if (!force && treeView.SelectedNode != null && treeView.SelectedNode.IsExpanded)
            {
                var n = treeView.SelectedNode;
                treeView.RefreshNode(treeView.SelectedNode);
                n.Expand();
            }
            else
            {
                foreach (TreeNode n in treeView.Nodes)
                    treeView.RefreshNode(n);
            }
        }

        public void AddDocumentType(string ext)
        {
            if (VisibleDocumentTypes == null)
                VisibleDocumentTypes = new List<String>();

            VisibleDocumentTypes.Add(ext.ToLower());
            Refresh(false);
        }

        public void RemoveDocumentType(string ext)
        {
            if (VisibleDocumentTypes != null)
            {
                VisibleDocumentTypes.Remove(ext.ToLower());
                Refresh(false);
            }
        }

        public void ToggleShowAllTypes()
        {
            ShowAllTypes = !ShowAllTypes;
            Refresh(false);
        }

        public void ToggleShowHiddenFolders()
        {
            ShowHiddenFolders = !ShowHiddenFolders;
            Refresh(false);
        }

        public bool HasDocumentType(string ext)
        {
            if (VisibleDocumentTypes == null)
                return false;

            return VisibleDocumentTypes.Contains(ext.ToLower());
        }

        public LazyTreeView TreeView
        {
            get { return treeView; }
        }

        #region State
        [StateItem]
        public List<String> VisibleDocumentTypes { get; set; }

        [StateItem]
        public bool ShowAllTypes { get; set; }

        [StateItem]
        public bool ShowHiddenFolders { get; set; }
        #endregion
    }
}
