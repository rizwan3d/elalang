using System;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using System.Text;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Linking
{
    internal sealed class LangModule : ForeignModule
    {
        public override void Initialize()
        {
            Add<ElaValue,ElaValue>("asInt", AsInt);
            Add<ElaValue,ElaValue>("asLong", AsLong);
            Add<ElaValue,ElaValue>("asSingle", AsSingle);
            Add<ElaValue,ElaValue>("asDouble", AsDouble);
            Add<ElaValue,ElaValue>("asBool", AsBool);
            Add<ElaValue,ElaValue>("asChar", AsChar);
            Add<ElaValue,ElaValue>("asString", AsString);
            Add<ElaValue,ElaTuple>("asTuple", AsTuple);
            Add<Int32,ElaRecord,ElaValue>("showRecordKey", ShowRecordKey);
        }

        public ElaValue ShowRecordKey(int field, ElaRecord rec)
        {
            var fl = rec.keys[field];

            if (fl.IndexOf(' ') != -1 || Format.IsSymbolic(fl))
                return new ElaValue("\"" + fl + "\"");

            return new ElaValue(fl);
        }

        public ElaValue AsString(ElaValue val)
        {
            if (val.TypeId > SysConst.MAX_TYP)
            {
                var tid = val.TypeId;
                var sb = new StringBuilder();
                var usb = val.Ref as ElaUserType2;
                var xs = new List<String>();

                while (usb != null)
                {
                    xs.Add(usb.Value2.DirectGetString());
                    var td = usb.Value1.Ref;

                    if (td.TypeId == tid)
                        usb = td as ElaUserType2;
                    else
                    {
                        usb = null;
                        xs.Add(td.ToString());
                    }
                }

                for (var i = xs.Count - 1; i > -1; i--)
                    sb.Append(xs[i]);

                return new ElaValue(sb.ToString());
            }

            return new ElaValue(val.AsString());
        }

        public ElaValue AsInt(ElaValue val)
        {
            return new ElaValue(val.GetInt());
        }

        public ElaValue AsLong(ElaValue val)
        {
            return new ElaValue(val.GetLong());
        }
        
        public ElaValue AsSingle(ElaValue val)
        {
            return new ElaValue(val.GetSingle());
        }

        public ElaValue AsDouble(ElaValue val)
        {
            return new ElaValue(val.GetDouble());
        }
        
        public ElaValue AsChar(ElaValue val)
        {
            return new ElaValue(val.GetChar());
        }
        
        public ElaValue AsBool(ElaValue val)
        {
            return new ElaValue(val.GetBool());
        }

        public ElaTuple AsTuple(ElaValue val)
        {
            if (val.TypeId == ElaMachine.LST)
            {
                var lst = (ElaList)val.Ref;
                var vals = new List<ElaValue>();

                foreach (var v in lst)
                    vals.Add(v);

                return new ElaTuple(vals.ToArray());
            }
            else
            {
                var rec = (ElaRecord)val.Ref;
                return new ElaTuple(rec.values);
            }
        }
    }
}
