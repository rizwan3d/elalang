using System;
using System.Collections.Generic;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class SetModule : ForeignModule
    {
        #region Construction
        public SetModule()
        {

        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add("empty", ElaSet.Empty);
            Add<IEnumerable<ElaValue>,ElaSet>("set", Create);
            Add<ElaValue,ElaSet,ElaSet>("add", Add);
            Add<ElaValue,ElaSet,ElaSet>("remove", Remove);
            Add<ElaValue,ElaSet,Boolean>("contains", Contains);
            Add<ElaSet,ElaList>("toList", s => s.ConvertToList());

            Add<ElaSet,String>("toString", s => s.ToString());
            Add<ElaSet,ElaValue>("setHead", s => s.Head());
            Add<ElaSet,ElaSet>("setTail", s => s.Tail());
            Add<ElaSet,ElaSet>("setNil", _ => ElaSet.Empty);
            Add<ElaSet,Boolean>("setIsNil", s => s.IsNil());
            Add<ElaSet,ElaValue,ElaSet>("setCons", (s,v) => s.Cons(s, v));
            Add<ElaSet,Int32>("setLength", s => s.Length);
            Add<ElaSet,ElaValue,ElaSet>("setGenerate", (s,v) => s.Generate(v));
        }


        public ElaSet Create(IEnumerable<ElaValue> seq)
        {
			return ElaSet.FromEnumerable(seq);
        }


        public ElaSet Add(ElaValue value, ElaSet set)
        {
			return set.Add(value);
        }


        public ElaSet Remove(ElaValue value, ElaSet set)
        {
			return set.Remove(value);
        }


        public bool Contains(ElaValue value, ElaSet set)
        {
			return set.Contains(value);
        }
        #endregion
    }
}