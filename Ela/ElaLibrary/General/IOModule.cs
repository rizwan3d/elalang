using System;
using System.IO;
using System.Text;
using Ela.Linking;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;

namespace Ela.Library.General
{
    public sealed class IOModule : ForeignModule
    {
        public IOModule()
        {

        }
        
        public override void Initialize()
        {
            Add<ElaFunction,String,String>("readLines", ReadLines);
            Add<String,ElaUnit>("truncateFile", TruncateFile);
            Add<String,String,ElaUnit>("writeLine", WriteLine);
            Add<String,String,ElaUnit>("writeText", WriteText);
        }
        
        public string ReadLines(ElaFunction fun, string file)
        {
            using (var sr = File.OpenText(file))
            {
                var line = String.Empty;
                var sb = new StringBuilder();

                while ((line = sr.ReadLine()) != null)
                    sb.AppendLine((String)fun.Call(new ElaValue(line)).AsObject());

                return sb.ToString();
            }
        }

        public ElaUnit TruncateFile(string file)
        {
            File.Open(file, FileMode.Create).Close();
            return ElaUnit.Instance;
        }

        public ElaUnit WriteLine(string line, string file)
        {
            using (var sw = new StreamWriter(File.Open(file, FileMode.Append)))
                sw.WriteLine(line);

            return ElaUnit.Instance;
        }

        public ElaUnit WriteText(string text, string file)
        {
            using (var sw = new StreamWriter(File.Open(file, FileMode.Create)))
                sw.WriteLine(text);

            return ElaUnit.Instance;
        }
    }
}
