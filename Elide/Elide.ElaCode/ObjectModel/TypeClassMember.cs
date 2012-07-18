using System.Text;
using Elide.CodeEditor.Infrastructure;

namespace Elide.ElaCode.ObjectModel
{
    public sealed class TypeClassMember : IClassMember
    {
        internal TypeClassMember(string name, int arguments, int signature)
        {
            Name = name;
            Arguments = arguments;
            Signature = signature;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);

            if (Signature != 0)
            {
                sb.Append(" ");

                for (var i = 0; i < Arguments; i++)
                {
                    if (i > 0)
                        sb.Append("->");

                    if ((Signature & (1 << i)) == (1 << i))
                        sb.Append("a");
                    else
                        sb.Append("_");
                }
            }

            return sb.ToString();
        }

        public string Name { get; private set; }

        public int Arguments { get; private set; }

        public int Signature { get; private set; }
    }
}
