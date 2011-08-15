using System;
using System.Collections.Generic;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class ArrayModule : ForeignModule
    {
        #region Construction
        private TypeId arrayTypeId;

        public ArrayModule()
        {

        }
        #endregion


        #region Methods
        public override void Initialize()
        {
			Add<ElaArray>("empty", CreateEmptyArray);
			Add<IEnumerable<ElaValue>,ElaArray>("array", CreateArray);
            Add<ElaValue,ElaArray,ElaUnit>("add", Add);
            Add<Int32,ElaArray,Boolean>("removeAt", RemoveAt);
            Add<Int32,ElaValue,ElaArray,Boolean>("insert", Insert);
            Add<ElaArray,ElaUnit>("clear", Clear);
            Add<Int32,ElaArray,ElaValue>("get", Get);
            Add<Int32,ElaValue,ElaArray,ElaUnit>("set", Set);

            Add<ElaValue,ElaValue,Boolean>("arrayEqual", (l,r) => l.ReferenceEquals(r));
            Add<ElaArray,Int32>("arrayLength", a => a.Length);
            Add<ElaArray,ElaValue,ElaArray>("arrayCons", Cons);
            Add<ElaArray,String>("toString", a => a.ToString());
            Add<ElaArray,ElaArray,ElaArray>("arrayConcat", Concatenate); 
            Add<ElaArray,ElaValue>("arrayHead", a => a.Head());
            Add<ElaArray,ElaArray>("arrayTail", a => a.Tail(arrayTypeId));
            Add<ElaArray,Boolean>("arrayIsNil", a => a.IsNil());
            Add<ElaValue,ElaArray,ElaArray>("arrayGenerate", (v,a) => a.Generate(v));
            Add<ElaArray,ElaArray>("arrayNil", _ => CreateEmptyArray());
        }


        public override void RegisterTypes(TypeRegistrator registrator)
        {
            arrayTypeId = registrator.ObtainTypeId("Array#");
        }


		public ElaArray CreateEmptyArray()
		{
			return new ElaArray(arrayTypeId);
		}


        public ElaArray CreateArray(IEnumerable<ElaValue> seq)
        {
            var lst = new List<ElaValue>();

            foreach (var v in seq)
                lst.Add(v);

            return new ElaArray(lst.ToArray(), arrayTypeId);
        }


        public ElaUnit Add(ElaValue value, ElaArray arr)
        {
            arr.Add(value);
            return ElaUnit.Instance;
        }


        public bool RemoveAt(int index, ElaArray arr)
        {
            return arr.Remove(index);
        }


        public bool Insert(int index, ElaValue value, ElaArray arr)
        {
            return arr.Insert(index, value);
        }


        public ElaUnit Clear(ElaArray arr)
        {
            arr.Clear();
            return ElaUnit.Instance;
        }


        public ElaValue Get(int index, ElaArray arr)
        {
            return arr.GetValue(index);
        }


        public ElaUnit Set(int index, ElaValue value, ElaArray arr)
        {
            arr.SetValue(index, value);
            return ElaUnit.Instance;
        }


        public ElaArray Concatenate(ElaArray left, ElaArray right)
        {
            var arr = new ElaValue[left.Length + right.Length];
            Array.Copy(left.GetRawArray(), 0, arr, 0, left.Length);
            Array.Copy(right.GetRawArray(), 0, arr, left.Length, right.Length);
            return new ElaArray(arr, arr.Length, 0, arrayTypeId);
        }


        public ElaArray Cons(ElaArray arr, ElaValue value)
        {
            if (arr.headIndex > 0)
            {
                var newArr = new ElaArray(arrayTypeId);
                arr.Copy(arr.headIndex, newArr);
                arr = newArr;
            }

            if (arr.Length == 0)
                arr.Add(value);
            else
                arr.Insert(0, value);

            return arr;
        }
        #endregion
    }
}