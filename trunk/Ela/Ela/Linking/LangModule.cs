using System;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using System.Text;
using System.Collections.Generic;

namespace Ela.Linking
{
    internal sealed class LangModule : ForeignModule
    {
        public override void Initialize()
        {
            Add<ElaList,String>("__stringFromList", StringFromList);
            Add<ElaList,ElaTuple>("__tupleFromList", TupleFromList);
            Add<ElaList,Boolean>("__isLazyList", IsLazyList);
            Add<ElaList,ElaList>("__reverseList", ReverseList);
        }

        public bool IsLazyList(ElaList lst)
        {
            return lst is ElaLazyList;
        }

        public ElaList ReverseList(ElaList list)
        {
            return list.Reverse();
        }

        public string StringFromList(ElaList lst)
        {
            var sb = new StringBuilder();

            foreach (var v in lst)
                sb.Append(v.DirectGetString());

            return sb.ToString();
        }

        public ElaTuple TupleFromList(ElaList lst)
        {
            var vals = new List<ElaValue>();

            foreach (var v in lst)
                vals.Add(v);

            return new ElaTuple(vals.ToArray());
        }
    }
}
