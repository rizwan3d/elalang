using System;
using Elide.Environment;
using System.IO;

namespace Elide.HelpViewer
{
    public sealed class HelpDocument : Document
    {
        public HelpDocument(string title) : base(title)
        {
            _title = title;
        }

        public HelpDocument(FileInfo fileInfo, string title) : base(fileInfo)
        {
            _title = title;
        }

        private string _title;
        public override string Title
        {
            get { return _title; }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { }
        }
    }
}
