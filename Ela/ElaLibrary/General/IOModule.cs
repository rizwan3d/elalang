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
    }
}
