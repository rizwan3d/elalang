using System;
using System.Windows.Forms;
using Elide.Forms;
using Elide.Workbench.Images;

namespace Elide.Workbench.Views
{
    public partial class OpenFilesControl : UserControl
    {
        public OpenFilesControl()
        {
            InitializeComponent();
            treeView.ImageList = imageList;
            treeView.ImageList.Images.Add("Folder", Bitmaps.Load<NS>("Folder"));
        }

        public TreeView TreeView
        {
            get { return treeView; }
        }
    }
}
