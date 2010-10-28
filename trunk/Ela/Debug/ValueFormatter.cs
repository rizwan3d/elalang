using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
using Ela.Runtime.Reflection;

namespace Ela.Debug
{
	public static class ValueFormatter
	{
		#region Methods
		public static string FormatValue(RuntimeValue value)
		{
			switch (value.DataType)
			{
				case ObjectType.String:
					return String.Format("\"{0}\"", value.ToString());
				case ObjectType.Char:
					return String.Format("'{0}'", value.ToString());
				case ObjectType.Array:
				case ObjectType.List:
				case ObjectType.Tuple:
				case ObjectType.Sequence:
					return FormatEnumerable(value.DataType, (ElaObject)value.Ref);
				case ObjectType.Record:
					return FormatRecord(value.DataType, (ElaRecord)value.Ref);
				case ObjectType.Function:
					var pc = ((ElaFunction)value.Ref).ParameterCount;
					return "function@" +  (pc < 0 ? "inf" : pc.ToString());
				case ObjectType.Lazy:
					var lazy = (ElaLazy)value.ToObject();
					return String.Format("lazy:{0}", lazy.HasValue() ? FormatValue(lazy.InternalValue) : "unevaluated");
				case ObjectType.Object:
					if (value.Ref is ElaInfo)
						return FormatType((ElaInfo)value.Ref);
					else
						return value.ToString();
				default:
					return value.ToString();
			}
		}


		private static string FormatType(ElaInfo type)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("{0}: {{\r\n", type.GetType().Name);
			var ti = type.GetTypeInfo();

			//foreach (var s in ti.GetFields().Except(ElaObject.Unit.GetTypeInfo().GetFields()))
			//{
			//    var val = ti.GetFieldValue(type, s);

			//    if (!(val.Ref is ElaFunction))
			//        sb.AppendFormat(" {0}: {1}\r\n", s, val);
			//}

			sb.AppendLine("}");
			return sb.ToString();
		}


		private static string FormatEnumerable(ObjectType type, ElaObject obj)
		{
			var sb = new StringBuilder();

			if (type == ObjectType.Array)
				sb.Append("[|");
			else if (type == ObjectType.Tuple)
			{
				sb.Append('(');
				var tag = String.Empty;

				if ((tag = ((ElaTuple)obj).Tag) != null)
					sb.AppendFormat(">{0} ", tag);
			}
			else if (type == ObjectType.Sequence)
				sb.Append("seq{");
			else
				sb.Append('[');

			var c = 0;

			foreach (var v in (IEnumerable<RuntimeValue>)obj)
			{
				if (c++ > 0)
					sb.Append(',');

				sb.Append(FormatValue(v));
			}

			if (type == ObjectType.Tuple)
				sb.Append(')');
			else if (type == ObjectType.Array)
				sb.Append("|]");
			else if (type == ObjectType.Sequence)
			{
				if (((ElaSequence)obj).Function != null)
					sb.Append("unevaluated");

				sb.Append('}');
			}
			else
				sb.Append(']');

			return sb.ToString();
		}


		private static string FormatRecord(ObjectType type, ElaRecord table)
		{
			var sb = new StringBuilder();
			sb.Append('(');

			var tag = String.Empty;

			if ((tag = ((ElaRecord)table).Tag) != null)
				sb.AppendFormat(">{0} ", tag);

			var c = 0;
		
			foreach (var k in table.GetKeys())
			{
				if (c++ > 0)
					sb.Append(',');

				sb.AppendFormat("{0}:{1}", k, FormatValue(table.GetField(k)));
			}

			sb.Append(')');
			return sb.ToString();
		}
		#endregion
	}
}
