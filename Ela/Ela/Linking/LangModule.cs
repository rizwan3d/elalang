using System;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using System.Text;
using System.Collections.Generic;

namespace Ela.Linking
{
    internal sealed class LangModule : ForeignModule
    {
        abstract class Cast : ElaFunction
        {
            internal Cast() : base(1) { }
        }

        sealed class ToInt : Cast
        {
            public override ElaValue Call(params ElaValue[] args)
            {
                var value = args[0];

                switch (value.TypeId)
                {
                    case ElaMachine.LNG:
                        return new ElaValue((Int32)value.Ref.AsLong());
                    case ElaMachine.REA:
                        return new ElaValue((Int32)value.DirectGetReal());
                    case ElaMachine.DBL:
                        return new ElaValue((Int32)value.Ref.AsDouble());
                    case ElaMachine.CHR:
                    case ElaMachine.BYT:
                        return new ElaValue(value.I4);
                    default:
                        return value;
                }
            }
        }

        sealed class ToLong : Cast
        {
            public override ElaValue Call(params ElaValue[] args)
            {
                var value = args[0];

                switch (value.TypeId)
                {
                    case ElaMachine.REA:
                        return new ElaValue((Int64)value.DirectGetReal());
                    case ElaMachine.DBL:
                        return new ElaValue((Int64)value.Ref.AsDouble());
                    case ElaMachine.INT:
                    case ElaMachine.CHR:
                    case ElaMachine.BYT:
                        return new ElaValue((Int64)value.I4);
                    default:
                        return value;
                }
            }
        }

        sealed class ToSingle : Cast
        {
            public override ElaValue Call(params ElaValue[] args)
            {
                var value = args[0];

                switch (value.TypeId)
                {
                    case ElaMachine.LNG:
                        return new ElaValue((Single)value.Ref.AsLong());
                    case ElaMachine.DBL:
                        return new ElaValue((Single)value.Ref.AsDouble());
                    case ElaMachine.INT:
                        return new ElaValue((Single)value.I4);
                    default:
                        return value;
                }
            }
        }

        sealed class ToDouble : Cast
        {
            public override ElaValue Call(params ElaValue[] args)
            {
                var value = args[0];

                switch (value.TypeId)
                {
                    case ElaMachine.LNG:
                        return new ElaValue((Double)value.Ref.AsLong());
                    case ElaMachine.REA:
                        return new ElaValue((Double)value.DirectGetReal());
                    case ElaMachine.INT:
                        return new ElaValue((Double)value.I4);
                    default:
                        return value;
                }
            }
        }

        sealed class ToChar : Cast
        {
            public override ElaValue Call(params ElaValue[] args)
            {
                var value = args[0];

                if (value.TypeId == ElaMachine.INT)
                    return new ElaValue((Char)value.I4);

                return value;
            }
        }

        sealed class ToBool : Cast
        {
            public override ElaValue Call(params ElaValue[] args)
            {
                var value = args[0];

                if (value.TypeId == ElaMachine.INT)
                    return new ElaValue(value.I4 == 1);
                else if (value.TypeId == ElaMachine.LNG)
                    return new ElaValue(value.Ref.AsLong() == 1);

                return value;
            }
        }

        sealed class ToStr : Cast
        {
            public override ElaValue Call(params ElaValue[] args)
            {
                var value = args[0];

                if (value.TypeId == ElaMachine.CHR)
                    return new ElaValue(((Char)value.I4).ToString());

                return value;
            }
        }

        public override void Initialize()
        {
            Add<ElaList,String>("__stringFromList", StringFromList);
            Add<ElaList,ElaTuple>("__tupleFromList", TupleFromList);
            Add<ElaList,Boolean>("__isLazyList", IsLazyList);
            Add<ElaList,ElaList>("__reverseList", ReverseList);
            Add<ElaList,Int32>("__listLength", ListLength);
            Add<ElaUserType,ElaValue>("__unwrap", Unwrap);

            Add("__toInt", new ToInt());
            Add("__toLong", new ToLong());
            Add("__toSingle", new ToSingle());
            Add("__toDouble", new ToDouble());
            Add("__toChar", new ToChar());
            Add("__toBool", new ToBool());
            Add("__toString", new ToStr());
        }

        public ElaValue Unwrap(ElaUserType type)
        {
            if (type.Values == null)
                return new ElaValue(ElaUnit.Instance);
            else
                return new ElaValue(new ElaTuple(type.Values));
        }

        public int ListLength(ElaList lst)
        {
            return lst.GetLength();
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
