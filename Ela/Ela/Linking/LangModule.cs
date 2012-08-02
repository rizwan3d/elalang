﻿using System;
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
            Add<ElaValue,ElaValue>("asInt", AsInt);
            Add<ElaValue,ElaValue>("asLong", AsLong);
            Add<ElaValue,ElaValue>("asSingle", AsSingle);
            Add<ElaValue,ElaValue>("asDouble", AsDouble);
            Add<ElaValue,ElaValue>("asBool", AsBool);
            Add<ElaValue,ElaValue>("asChar", AsChar);
            Add<ElaValue,ElaValue>("asString", AsString);
            Add<ElaValue,ElaTuple>("asTuple", AsTuple);
        }

        public ElaValue AsString(ElaValue val)
        {
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
