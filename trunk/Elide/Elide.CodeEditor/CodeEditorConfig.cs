using System;
using Elide.Environment.Configuration;

namespace Elide.CodeEditor
{
    [Serializable]
    public class CodeEditorConfig : Config
    {
        public CodeEditorConfig()
        {
            EnableBackgroundCompilation = true;
            EnableFolding = true;
            HighlightErrors = true;
            AutocompleteChars = "([.";
        }

        public bool EnableBackgroundCompilation { get; set; }

        public bool MatchBraces { get; set; }

        public bool EnableFolding { get; set; }

        public bool HighlightErrors { get; set; }

        public bool ShowAutocompleteOnSpace { get; set; }

        public bool ShowAutocompleteOnChars { get; set; }

        public string AutocompleteChars { get; set; }
    }
}
