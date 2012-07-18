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
            Add<ElaValue,ElaValue>("__unwrap", Unwrap);
            Add<ElaValue,ElaRecord>("__getTypeInfo", GetTypeInfo);
            Add<ElaModule,ElaValue,Boolean>("__isInfix", IsInfix);
            Add<ElaModule,ElaValue,ElaValue,Int32>("__getParameterIndex", GetVariantParameterIndex);
            Add<ElaValue,Int32>("__constructorIndex", GetConstructorIndex);
            Add<ElaUserType,ElaTuple,ElaObject>("__assemble", Assemble);
            Add<ElaModule,ElaValue,ElaObject>("__conssucc", ConstructorSuccessor);
            Add<ElaModule,ElaValue,ElaObject>("__conspred", ConstructorPredecessor);
            Add<ElaModule,ElaValue,ElaValue>("__constoint", ConstructorToInt);
            Add<ElaModule,ElaValue,ElaValue,ElaObject>("__consfromint", ConstructorFromInt);
            Add<ElaModule,ElaValue,ElaValue>("__consmax", ConstructorMaxValue);

            Add("__toInt", new ToInt());
            Add("__toLong", new ToLong());
            Add("__toSingle", new ToSingle());
            Add("__toDouble", new ToDouble());
            Add("__toChar", new ToChar());
            Add("__toBool", new ToBool());
            Add("__toString", new ToStr());
        }

        public ElaValue ConstructorMaxValue(ElaModule mod, ElaValue val)
        {
            if (val.TypeId <= SysConst.MAX_TYP)
                throw new ElaRuntimeException(ElaRuntimeError.InvalidType, "?", val.GetTypeName());

            var type = (ElaUserType)val.Ref;
            var asm = mod.GetCurrentMachine().Assembly;
            var typeInfo = asm.Types[val.TypeId];
            return new ElaValue(typeInfo.Constructors.Count - 1);
        }

        public ElaValue ConstructorToInt(ElaModule mod, ElaValue val)
        {
            if (val.TypeId <= SysConst.MAX_TYP)
                throw new ElaRuntimeException(ElaRuntimeError.InvalidType, "?", val.GetTypeName());

            var type = (ElaUserType)val.Ref;
            var asm = mod.GetCurrentMachine().Assembly;
            var typeInfo = asm.Types[val.TypeId];

            return new ElaValue(typeInfo.Constructors.IndexOf(type.GetTag(null)));
        }

        public ElaObject ConstructorFromInt(ElaModule mod, ElaValue val, ElaValue idx)
        {
            if (idx.TypeId != ElaMachine.INT)
                throw new ElaRuntimeException(ElaRuntimeError.InvalidType, TCF.GetShortForm(ElaTypeCode.Integer), idx.GetTypeName());

            if (val.TypeId <= SysConst.MAX_TYP)
                throw new ElaRuntimeException(ElaRuntimeError.InvalidType, "?", val.GetTypeName());
            
            var type = (ElaUserType)val.Ref;
            var asm = mod.GetCurrentMachine().Assembly;
            var typeInfo = asm.Types[val.TypeId];

            if (idx.I4 < 0 || idx.I4 >= typeInfo.Constructors.Count)
                throw new ElaRuntimeException(ElaRuntimeError.IndexOutOfRange, idx.I4, TCF.GetShortForm(ElaTypeCode.Integer),
                    val.ToString(), val.GetTypeName());

            return CreateConstructor(asm, typeInfo.Constructors[idx.I4], val.TypeId);
        }

        public ElaObject ConstructorSuccessor(ElaModule mod, ElaValue val)
        {
            return ConstructorSuccPred(mod, val, 1);
        }

        public ElaObject ConstructorPredecessor(ElaModule mod, ElaValue val)
        {
            return ConstructorSuccPred(mod, val, -1);
        }

        private ElaObject ConstructorSuccPred(ElaModule mod, ElaValue val, int plus)
        {
            if (val.TypeId <= SysConst.MAX_TYP)
                throw new ElaRuntimeException(ElaRuntimeError.InvalidType, "?", val.GetTypeName());

            var type = (ElaUserType)val.Ref;
            var asm = mod.GetCurrentMachine().Assembly;
            var typeInfo = asm.Types[val.TypeId];

            var next = typeInfo.Constructors.IndexOf(type.GetTag(null)) + plus;

            if (next < typeInfo.Constructors.Count && next > -1)
                return CreateConstructor(asm, typeInfo.Constructors[next], val.TypeId);

            throw new ElaRuntimeException(ElaRuntimeError.ConstructorSequenceError, asm.Constructors[type.GetTag(null)].Name);
        }

        private ElaUserType CreateConstructor(CodeAssembly asm, int id, int typeId)
        {
            var cons = asm.Constructors[id];
            
            if (cons.Parameters == null)
                return new ElaUserType(cons.TypeName, typeId, cons.Code, new ElaValue(ElaUnit.Instance));
            else
            {
                var t = new ElaTuple(cons.Parameters.Count);

                for (var i = 0; i < cons.Parameters.Count; i++)
                    t.InternalSetValue(i, new ElaValue(ElaUnit.Instance));

                return new ElaUserType(cons.TypeName, typeId, cons.Code, new ElaValue(t));
            }
        }

        public ElaObject Assemble(ElaUserType type, ElaTuple tup)
        {
            return new ElaUserType(type.GetTypeName(), type.TypeId, type.GetTag(null), new ElaValue(tup));
        }

        public int GetConstructorIndex(ElaValue val)
        {
            return val.Ref.GetTag(null);
        }

        public int GetVariantParameterIndex(ElaModule mod, ElaValue val, ElaValue fieldVal)
        {
            if (val.TypeId <= SysConst.MAX_TYP)
                return -1;
            
            var type = (ElaUserType)val.Ref;
            var tag = mod.GetCurrentMachine().Assembly.Constructors[type.GetTag(null)];

            if (tag.Parameters == null)
                return -1;
            
            if (fieldVal.TypeId == ElaMachine.STR)
                return tag.Parameters.IndexOf(fieldVal.DirectGetString());

            if (fieldVal.TypeId == ElaMachine.INT)
                return fieldVal.I4;

            return -1;
        }

        public bool IsInfix(ElaModule mod, ElaValue val)
        {
            if (val.TypeId <= SysConst.MAX_TYP)
                return false;

            var type = (ElaUserType)val.Ref;
            var tag = mod.GetCurrentMachine().Assembly.Constructors[type.GetTag(null)];

            return Ela.CodeModel.Format.IsSymbolic(tag.Name) && type.Values != null &&
                type.Values.Length == 2;
        }

        public ElaRecord GetTypeInfo(ElaValue val)
        {
            return new ElaRecord(
                new ElaRecordField("typeName", val.GetTypeName()),
                new ElaRecordField("typeCode", val.TypeId));
        }

        public ElaValue Unwrap(ElaValue val)
        {
            if (val.TypeId <= SysConst.MAX_TYP)
                return new ElaValue(ElaUnit.Instance);

            var type = (ElaUserType)val.Ref;

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
