using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Elide.Core;
using Elide.Environment;
using Elide.Environment.Editors;

namespace Elide.Workbench
{
    public sealed class DialogService : Service, IDialogService
    {
        private const string FILTER = "All files (*.*)|*.*";
        private const string OPEN_TITLE = "Open File";
        private const string SAVE_TITLE = "Save File";

        private OpenFileDialog openDialog;
        private SaveFileDialog saveDialog;
        
        public DialogService()
        {
            
        }
        
        public FileInfo ShowSaveDialog(string fileName)
        {
            InitializeDialogs();
            saveDialog.FileName = fileName;
            var res = saveDialog.ShowDialog(WB.Form);
            return res == DialogResult.OK ? new FileInfo(saveDialog.FileName) : null;
        }

        public IEnumerable<FileInfo> ShowOpenDialog(bool multiple)
        {
            InitializeDialogs();
            openDialog.Multiselect = multiple;
            var res = openDialog.ShowDialog(WB.Form);

            if (res == DialogResult.OK)
                return openDialog.FileNames.Select(f => new FileInfo(f));
            else
                return null;
        }

        public bool? ShowPromptDialog(string text, params object[] args)
        {
            var res = default(DialogResult);
            WB.Form.Invoke(() =>
                {
                    res = MessageBox.Show(WB.Form, String.Format(text, args),
                        Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                });
            return res == DialogResult.Cancel ? (Boolean?)null : (Boolean?)(res == DialogResult.Yes);
        }

        public bool ShowWarningDialog(string text, params object[] args)
        {
            var res = default(DialogResult);
            WB.Form.Invoke(
                () =>
                {
                    res = MessageBox.Show(WB.Form, String.Format(text, args),
                        Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                });
            return res == DialogResult.OK;
        }

        private void InitializeDialogs()
        {
            if (openDialog != null && saveDialog != null)
                return;

            var serv = App.GetService<IEditorService>();
            var sb = new StringBuilder(FILTER);
            var idx = 1;
            var selIdx = 1;

            foreach (var e in serv.EnumerateInfos("editors").OfType<EditorInfo>())
            {
                if (!e.Flags.Set(EditorFlags.HiddenEverywhere))
                {
                    sb.AppendFormat("|{0} (*{1})|*{1}", e.FileExtensionDescription, e.FileExtension);

                    if (e.Flags.Set(EditorFlags.Main))
                        selIdx = idx + 1;

                    idx++;
                }
            }

            openDialog = new OpenFileDialog
            {
                Filter = sb.ToString(),
                FilterIndex = selIdx,
                RestoreDirectory = true,
                Title = OPEN_TITLE
            };

            saveDialog = new SaveFileDialog
            {
                Filter = sb.ToString(),
                FilterIndex = selIdx,
                RestoreDirectory = true,
                Title = SAVE_TITLE
            };
        }
    }
}
