using System;
using System.Collections.Generic;
using System.IO;
using Elide;
using Ela.Compilation;
using Elide.Scintilla.ObjectModel;
using Elide.TextEditor;
using Elide.CodeEditor;

namespace Elide.ElaCode
{
    public sealed class ElaDocument : CodeDocument
    {
        internal ElaDocument(FileInfo fileInfo, SciDocument sciDoc) : base(fileInfo, sciDoc)
        {

        }

        internal ElaDocument(string title, SciDocument sciDoc) : base(title, sciDoc)
        {

        }

        internal new SciDocument GetSciDocument()
        {
            return base.GetSciDocument();
        }
    }
}
