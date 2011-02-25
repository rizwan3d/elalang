using System;
using System.Collections.Generic;
using System.Linq;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class ArrayModule : ForeignModule
    {
        #region Construction
        public ArrayModule()
        {

        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<IEnumerable<ElaValue>,ElaArray>("array", CreateArray);
            Add<ElaValue,ElaArray,ElaUnit>("add", Add);
            Add<Int32,ElaArray,ElaUnit>("removeAt", RemoveAt);
            Add<Int32,ElaValue,ElaArray,ElaUnit>("insert", Insert);
            Add<ElaArray,ElaUnit>("clear", Clear);
            Add<Int32,ElaArray,ElaVariant>("get", Get);
        }


        public ElaArray CreateArray(IEnumerable<ElaValue> seq)
        {
            return new ElaArray(seq.ToArray());
        }


        public ElaUnit Add(ElaValue value, ElaArray arr)
        {
            arr.Add(value);
            return ElaUnit.Instance;
        }


        public ElaUnit RemoveAt(int index, ElaArray arr)
        {
            arr.Remove(index);
            return ElaUnit.Instance;
        }


        public ElaUnit Insert(int index, ElaValue value, ElaArray arr)
        {
            arr.Insert(index, value);
            return ElaUnit.Instance;
        }


        public ElaUnit Clear(ElaArray arr)
        {
            arr.Clear();
            return ElaUnit.Instance;
        }


        public ElaVariant Get(int index, ElaArray arr)
        {
            if (index < 0 || index >= arr.Length)
                return ElaVariant.None();

            return ElaVariant.Some(arr.FastGet(index));
        }
        #endregion
    }
}