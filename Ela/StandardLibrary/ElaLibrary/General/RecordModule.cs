using System;
using Ela.Linking;
using Ela.Runtime.ObjectModel;
using System.Collections.Generic;

namespace Ela.Library.General
{
    public sealed class RecordModule : ForeignModule
    {
        public RecordModule()
        {

        }

        public override void Initialize()
        {
            Add<String,ElaRecord,Boolean>("hasField", HasField);
            Add<ElaRecord,ElaRecord,ElaRecord>("addFields", AddFields);
            Add<IEnumerable<String>,ElaRecord,ElaRecord>("removeFields", RemoveFields);
            Add<ElaRecord,ElaList>("fields", GetFields);
        }

        public bool HasField(string field, ElaRecord rec)
        {
            return rec.HasField(field);
        }

        public ElaRecord AddFields(ElaRecord fields, ElaRecord rec)
        {
            var fieldList = new List<ElaRecordField>();
            fieldList.AddRange(rec);
            fieldList.AddRange(fields);
            return new ElaRecord(fieldList.ToArray());
        }

        public ElaRecord RemoveFields(IEnumerable<String> fields, ElaRecord rec)
        {
            var fieldList = new List<ElaRecordField>();
            var fieldArr = new List<String>(fields);

            foreach (var f in rec)
                if (fieldArr.IndexOf(f.Field) == -1)
                    fieldList.Add(f);

            return new ElaRecord(fieldList.ToArray());
        }

        public ElaList GetFields(ElaRecord rec)
        {
            return ElaList.FromEnumerable(rec.GetKeys());
        }
    }
}
