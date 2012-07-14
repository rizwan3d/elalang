using System;
using Ela.CodeModel;
using Ela.Linking;
using Ela.Parsing;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
    public sealed class ReadModule : ForeignModule
    {
        public ReadModule()
        {

        }
        
        public override void Initialize()
        {
            Add<string,ElaValue>("readStr", Read);
        }
        
        public ElaValue Read(string str)
        {
            var p = new ElaParser();
            var res = p.Parse(str);

            if (!res.Success)
                throw Fail(str);

            var prog = res.Program;

            if (prog.Instances != null
                || prog.Includes != null
                || prog.Classes != null
                || prog.Types != null
                || prog.TopLevel.Equations.Count != 1)
                throw Fail(str);

            var eq = prog.TopLevel.Equations[0];

            if (eq.Right != null)
                throw Fail(str);

            return Read(eq.Left, str);
        }

        private ElaValue Read(ElaExpression exp, string str)
        {
            switch (exp.Type)
            {
                case ElaNodeType.ListLiteral:
                    {
                        var n = (ElaListLiteral)exp;
                        var arr = new ElaValue[n.Values.Count];

                        for (var i = 0; i < arr.Length; i++)
                            arr[i] = Read(n.Values[i], str);

                        return new ElaValue(ElaList.FromEnumerable(arr));
                    }
                case ElaNodeType.TupleLiteral:
                    {
                        var n = (ElaTupleLiteral)exp;
                        var arr = new ElaValue[n.Parameters.Count];

                        for (var i = 0; i < arr.Length; i++)
                            arr[i] = Read(n.Parameters[i], str);

                        return new ElaValue(new ElaTuple(arr));
                    }
                case ElaNodeType.RecordLiteral:
                    {
                        var n = (ElaRecordLiteral)exp;
                        var arr = new ElaRecordField[n.Fields.Count];

                        for (var i = 0; i < arr.Length; i++)
                        {
                            var f = n.Fields[i];
                            arr[i] = new ElaRecordField(f.FieldName, Read(f.FieldValue, str));                           
                        }

                        return new ElaValue(new ElaRecord(arr));
                    }
                case ElaNodeType.Primitive:
                    {
                        var n = (ElaPrimitive)exp;

                        switch (n.Value.LiteralType)
                        {
                            case ElaTypeCode.Integer:
                                return new ElaValue(n.Value.AsInteger());
                            case ElaTypeCode.Single:
                                return new ElaValue(n.Value.AsReal());
                            case ElaTypeCode.Double:
                                return new ElaValue(n.Value.AsDouble());
                            case ElaTypeCode.Long:
                                return new ElaValue(n.Value.AsLong());
                            case ElaTypeCode.Char:
                                return new ElaValue(n.Value.AsChar());
                            case ElaTypeCode.Boolean:
                                return new ElaValue(n.Value.AsBoolean());
                            case ElaTypeCode.String:
                                return new ElaValue(n.Value.AsString());
                            default:
                                throw Fail(str);
                        }
                    }
                case ElaNodeType.UnitLiteral:
                    return new ElaValue(ElaUnit.Instance);
                default:
                    throw Fail(str);
            }
        }

        private Exception Fail(string str)
        {
            return new ElaRuntimeException("Failure",
                String.Format("Unable to read a literal from a string \"{0}\".", str));
        }
    }
}
