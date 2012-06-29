using System;
using System.Collections.Generic;
using System.Linq;
using Ela;
using Ela.Compilation;
using Ela.Parsing;
using Elide.CodeEditor;
using Elide.CodeEditor.Infrastructure;
using Elide.CodeEditor.Views;
using Elide.ElaCode.ObjectModel;

namespace Elide.ElaCode
{
    public sealed class ElaBackgroundCompiler : IBackgroundCompiler
    {
        public Tuple<ICompiledUnit,IEnumerable<MessageItem>> Compile(CodeDocument doc, string source)
        {
            var par = new ElaParser();
            var parRes = par.Parse(source);
            var msg = new List<MessageItem>();
            var unit = default(ICompiledUnit);
            Func<ElaMessage,MessageItem> project = m => new MessageItem(
                m.Type == MessageType.Error ? MessageItemType.Error : MessageItemType.Warning, m.Message, doc, m.Line, m.Column);

            if (parRes.Success)
            {
                var copt = new CompilerOptions();
                //copt.NoWarnings = true;
                copt.ShowHints = false;
                copt.GenerateDebugInfo = true;
                copt.IgnoreUndefined = true;
                var comp = new ElaCompiler();
               
                try
                {
                    var compRes = comp.Compile(parRes.Program, copt, new ExportVars());
                    msg.AddRange(compRes.Messages.Where(m => m.Type != MessageType.Hint).Select(project));
                    unit = compRes.CodeFrame != null ? new CompiledUnit(doc, compRes.CodeFrame) : null;
                }
                catch { }
            }
            else
                msg.AddRange(parRes.Messages.Select(project));

            return Tuple.Create(unit, (IEnumerable<MessageItem>)msg);
        }
    }
}
