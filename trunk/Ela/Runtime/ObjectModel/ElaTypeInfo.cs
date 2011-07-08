using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaTypeInfo
	{
		#region Construction
        private List<String> keys;
        private List<ElaValue> values;

		internal ElaTypeInfo()
		{
            keys = new List<String>();
            values = new List<ElaValue>();
		}
		#endregion


		#region Methods
		public void AddField(string field, ElaValue value)
		{
			keys.Add(field);
			values.Add(value);
		}


		public void AddField<T>(string field, T value)
		{
			keys.Add(field);
			values.Add(ElaValue.FromObject(value));
		}


        internal ElaRecord ToRecord()
        {
            var rec = new ElaRecord(keys.Count);

            for (var i = 0; i < keys.Count; i++)
                rec.AddField(keys[i], false, values[i]);

            return rec;
        }
        #endregion
    }
}