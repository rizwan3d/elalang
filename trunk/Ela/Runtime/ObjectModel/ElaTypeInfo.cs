using System;
using System.Collections.Generic;
using System.Text;
using Ela.Runtime.ObjectModel;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaTypeInfo : ElaObject
	{
		#region Construction
        private const string TYPE_NAME = "typeInfo";
        private List<String> keys;
        private List<ElaValue> values;

		internal ElaTypeInfo() : base(ElaTraits.Eq|ElaTraits.Show|ElaTraits.FieldGet|ElaTraits.Get|ElaTraits.Len|ElaTraits.Seq)
		{
            keys = new List<String>();
            values = new List<ElaValue>();
		}
		#endregion


        #region Traits
        protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var li = default(ElaTypeInfo);
            var ri = default(ElaTypeInfo);

            if (!Cast(left, right, out li, out ri, ElaTraits.Eq, ctx))
                return Default();

            return new ElaValue(CompareInfos(li, ri));
        }


        protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            var li = default(ElaTypeInfo);
            var ri = default(ElaTypeInfo);

            if (!Cast(left, right, out li, out ri, ElaTraits.Eq, ctx))
                return Default();

            return new ElaValue(!CompareInfos(li, ri));
        }


        protected internal override string Show(ExecutionContext ctx, ShowInfo info)
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
                sb.AppendFormat("{0}{1}={2}\r\n", pad, keys[i], values[i].Show(ctx, f));
            }

            sb.Append(finPad + "}");
            return sb.ToString();
        }


        protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
        {
            if (index.TypeCode == ElaTypeCode.String)
                return GetField(index.AsString(), ctx);
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


        protected internal override ElaValue GetLength(ExecutionContext ctx)
        {
            return new ElaValue(keys.Count);
        }


        private bool CompareInfos(ElaTypeInfo left, ElaTypeInfo right)
        {
            return left.GetTypeName() == right.GetTypeName() &&
                CompareLists(left.keys, right.keys) &&
                CompareLists(left.values, right.values);
        }


        private bool CompareLists<T>(List<T> left, List<T> right) where T : IEquatable<T>
        {
            if (left.Count != right.Count)
                return false;

            for (var i = 0; i < left.Count; i++)
                if (!left[i].Equals(right[i]))
                    return false;

            return true;
        }


        private bool Cast(ElaValue left, ElaValue right, out ElaTypeInfo li, out ElaTypeInfo ri, ElaTraits trait, ExecutionContext ctx)
        {
            li = left.As<ElaTypeInfo>();
            ri = right.As<ElaTypeInfo>();

            if (li == null)
            {
                ctx.InvalidLeftOperand(left, right, trait);
                return false;
            }

            if (ri == null)
            {
                ctx.InvalidRightOperand(left, right, trait);
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


        #region Methods
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
    }
}