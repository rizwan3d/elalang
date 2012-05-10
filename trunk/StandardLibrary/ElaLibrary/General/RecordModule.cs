using System;
using System.Collections.Generic;
using System.Text;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
    public sealed class RecordModule : ForeignModule
    {
        #region Construction
        public RecordModule()
        {

        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<String,ElaRecord,Boolean>("hasField", HasField);
        }

        public bool HasField(string field, ElaRecord rec)
        {
            return rec.HasField(field);
        }
        #endregion
    }
}
