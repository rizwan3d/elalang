using System;
using System.Collections.Generic;
using System.Text;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaTypeInfo : ElaObject
	{
		#region Construction
        private const string TYPE_NAME = "typeInfo";
        private List<String> keys;
        private List<ElaValue> values;

		internal ElaTypeInfo()
		{
            keys = new List<String>();
            values = new List<ElaValue>();
		}
		#endregion


		#region Methods
		public override ElaPatterns GetSupportedPatterns()
		{
			return ElaPatterns.Record|ElaPatterns.Tuple;
		}


		protected internal override string GetTypeName()
		{
			return TYPE_NAME;
		}


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
		#endregion


        #region Operations
		//protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		//{
		//    var li = default(ElaTypeInfo);
		//    var ri = default(ElaTypeInfo);

		//    if (!Cast(left, right, out li, out ri, "equal", ctx))
		//        return Default();

		//    return new ElaValue(CompareInfos(li, ri));
		//}


		//protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		//{
		//    var li = default(ElaTypeInfo);
		//    var ri = default(ElaTypeInfo);

		//    if (!Cast(left, right, out li, out ri, "notequal", ctx))
		//        return Default();

		//    return new ElaValue(!CompareInfos(li, ri));
		//}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
        {
            var sb = new StringBuilder();
            sb.Append(GetTypeName());
            sb.AppendLine(" {");
            info = info ?? ShowInfo.Default;
            var len = info.Format != null ? info.Format.Length : 0;
            var pad = new String(' ', len);
            var finPad = len > 2 ? new String(' ', len - 2) : String.Empty;
            var typeFormat = new ShowInfo(0, 0, pad + "  ");

            for (var i = 0; i < keys.Count; i++)
            {
                var f = values[i].Is<ElaTypeInfo>() ? typeFormat : ShowInfo.Default;
                sb.AppendFormat("{0}{1}={2}\r\n", pad, keys[i], values[i].Show(f, ctx));
            }

            sb.Append(finPad + "}");
            return sb.ToString();
        }


		protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
        {
            if (index.TypeCode == ElaTypeCode.String)
                return GetField(index.DirectGetString(), ctx);
            else if (index.TypeCode == ElaTypeCode.Integer)
            {
                var idx = index.AsInteger();

                if (idx < 0 || idx >= keys.Count)
                {
                    ctx.IndexOutOfRange(index, new ElaValue(this));
                    return Default();
                }

                return values[idx];
            }
            
            ctx.InvalidIndexType(index);
            return Default();
        }


		protected internal override ElaValue GetField(string field, ExecutionContext ctx)
        {
            var idx = GetOrdinal(field);

            if (idx == -1)
            {
                ctx.UnknownField(field, new ElaValue(this));
                return Default();
            }

            return values[idx];
        }


        private bool CompareInfos(ElaTypeInfo left, ElaTypeInfo right)
        {
            return left.GetTypeName() == right.GetTypeName() &&
                EqHelper.ListEquals(left.keys, right.keys) &&
                EqHelper.ListEquals(left.values, right.values);
        }


        private bool Cast(ElaValue left, ElaValue right, out ElaTypeInfo li, out ElaTypeInfo ri, string op, ExecutionContext ctx)
        {
            li = left.As<ElaTypeInfo>();
            ri = right.As<ElaTypeInfo>();

            if (li == null)
            {
                ctx.InvalidLeftOperand(left, right, op);
                return false;
            }

            if (ri == null)
            {
                ctx.InvalidRightOperand(left, right, op);
                return false;
            }

            return true;
        }


        private int GetOrdinal(string key)
        {
            for (var i = 0; i < keys.Count; i++)
                if (keys[i] == key)
                    return i;

            return -1;
        }
        #endregion
    }
}