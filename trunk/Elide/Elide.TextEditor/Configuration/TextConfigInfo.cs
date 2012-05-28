using System;
using Elide.Core;

namespace Elide.TextEditor.Configuration
{
    public sealed class TextConfigInfo : ExtInfo
    {
        public TextConfigInfo(string key, string display, TextConfigOptions options) : base(key)
        {
            Display = display;
            Options = options;
        }

        public override string ToString()
        {
            return Display;
        }

        public TextConfigOptions Options { get; private set; }

        public string Display { get; private set; }
    }
}
