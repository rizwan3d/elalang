using System;
using System.Collections.Generic;
using System.Text;

namespace ElaConsole
{
    internal static class IndentHelper
    {
        internal static int GetIndent(string line)
        {
            line = line ?? String.Empty;
            var upc = line.ToUpper();
            var trim = upc.TrimStart(' ');

            if (trim.StartsWith("LET"))
                return GetIndentForBinding(upc.Substring(3).TrimStart(' '), line);
            else if (trim.StartsWith("WHERE"))
                return GetIndentForBinding(upc.Substring(5).TrimStart(' '), line);
            else
                return upc.Length - trim.Length;
        }


        private static int GetIndentForBinding(string trim, string orig)
        {
            if (trim.StartsWith("PRIVATE"))
                return GetIndentForBinding(trim.Substring(7).TrimStart(' '), orig);
            else if (trim.StartsWith("INLINE"))
                return GetIndentForBinding(trim.Substring(6).TrimStart(' '), orig);
            else
                return orig.Length - trim.Length;
        }
    }
}
