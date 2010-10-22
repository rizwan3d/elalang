using System;
using System.Collections.Generic;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public class ElaRecord : ElaTuple
	{
		#region Construction
		private string[] keys;
		
		public ElaRecord(params ElaRecordField[] fields) : base(fields.Length, ObjectType.Record)
		{
			for (var i = 0; i < fields.Length; i++)
				AddField(i, fields[i].Field, fields[i].Value);
		}


		internal ElaRecord(int size) : base(size, ObjectType.Record)
		{
			keys = new string[size];
		}
		#endregion


		#region Methods
		public override ElaTypeInfo GetTypeInfo()
		{
			return new ElaRecordInfo(Tag);
		}


		public IEnumerable<String> GetKeys()
		{
			return keys;
		}


		public RuntimeValue GetField(string key)
		{
			var res = GetValue(key);

			if (res.Type == ElaMachine.___)
				throw new IndexOutOfRangeException();

			return res;
		}


		public bool HasField(string key)
		{
			return GetOrdinal(key) != -1;
		}


		public virtual bool SetField(string key, RuntimeValue value)
		{
			return SetValue(key, value);
		}


		protected internal override RuntimeValue GetValue(RuntimeValue key)
		{
			return key.Type == ElaMachine.STR ? GetValue(key.Ref.ToString()) : base.GetValue(key);
		}


		protected internal override bool SetValue(RuntimeValue index, RuntimeValue value)
		{
			return index.Type == ElaMachine.STR ? SetValue(index.Ref.ToString(), value) : 
				base.SetValue(index, value);
		}		


		internal void AddField(int index, string key, RuntimeValue value)
		{
			keys[index] = key;
			Values[index] = value;
		}


		private int GetOrdinal(string key)
		{
			for (var i = 0; i < keys.Length; i++)
				if (keys[i] == key)
					return i;

			return -1;
		}


		private bool SetValue(string key, RuntimeValue value)
		{
			var idx = GetOrdinal(key);

			if (idx > -1 && idx < Values.Length)
			{
				Values[idx] = value;
				return true;
			}
			else
				return false;
		}


		private RuntimeValue GetValue(string key)
		{
			var index = GetOrdinal(key);
			return index != -1 && index < Values.Length ? Values[index] : new RuntimeValue(Invalid);
		}
		#endregion


		#region Properties
		public RuntimeValue this[string key]
		{
			get { return GetValue(new RuntimeValue(key)); }
			set { SetField(key, value); }
		}


		public new RuntimeValue this[int index]
		{
			get { return GetValue(new RuntimeValue(index)); }
			set { SetValue(new RuntimeValue(index), value); }
		}

		internal override bool ReadOnly { get { return false; } }
		#endregion
	}
}
