using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class ElaMap : ElaObject, IEnumerable<ElaValue>
    {
        #region Construction
        private const string TAG = "Map#";
        
        internal ElaMap(AvlTree tree, TypeId typeId) : base(typeId)
        {
            Tree = tree;
        }
        #endregion


        #region Methods
        public override string GetTag()
		{
			return TAG;
		}


		public ElaMap Add(ElaValue key, ElaValue value)
        {
            return new ElaMap(Tree.Add(key, value), new TypeId(base.TypeId));
        }


        public ElaMap Remove(ElaValue key)
        {
            return new ElaMap(Tree.Remove(key), new TypeId(base.TypeId));
        }


        public bool Contains(ElaValue key)
        {
            return !Tree.Search(key).IsEmpty;
        }


        public IEnumerator<ElaValue> GetEnumerator()
        {
            foreach (var kv in Tree.Enumerate())
                yield return kv.Value;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ElaValue>)this).GetEnumerator();
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("map");
            sb.Append('{');
            var c = 0;

            foreach (var e in Tree.Enumerate())
            {
                if (c++ > 0)
                    sb.Append(',');

                sb.Append(e.Key.ToString());
                sb.Append('=');
                sb.Append(e.Value.ToString());
            }

            sb.Append('}');
            return sb.ToString();
        }


        public ElaValue GetValue(ElaValue index)
        {
            var res = Tree.Search(index);

            if (res.IsEmpty)
                throw new ElaRuntimeException(ElaRuntimeError.IndexOutOfRange, index);
            
            return res.Value;
        }


        public ElaRecord ConvertToRecord()
        {
            var fields = new ElaRecordField[Length];
            var c = 0;

            foreach (var kv in Tree.Enumerate())
                fields[c++] = new ElaRecordField(kv.Key.ToString(), kv.Value, false);

            return new ElaRecord(fields);
        }
        #endregion


        #region Properties
        internal AvlTree Tree { get; private set; }

        public int Length
        {
            get
            {
                var c = 0;

                foreach (var _ in Tree.Enumerate())
                    c++;

                return c;
            }
        }
        #endregion
    }		
}
