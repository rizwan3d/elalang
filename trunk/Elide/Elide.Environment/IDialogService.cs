using System;
using System.Collections.Generic;
using System.IO;
using Elide.Core;

namespace Elide.Environment
{
    public interface IDialogService : IService
    {
        FileInfo ShowSaveDialog(string fileName);

        IEnumerable<FileInfo> ShowOpenDialog(bool multiple);

        bool? ShowPromptDialog(string text, params object[] args);

        bool ShowWarningDialog(string text, params object[] args);
    }
}
