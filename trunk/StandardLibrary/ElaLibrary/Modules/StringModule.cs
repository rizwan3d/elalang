using System;
using System.Linq;
using Ela.ModuleGenerator;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using System.Collections.Generic;
using System.Text;

namespace Ela.StandardLibrary.Modules
{
	public partial class StringModule
	{
		#region Construction
		public StringModule()
		{

		}
		#endregion


		#region Nested Classes
		internal sealed class ElaStringBuilder : ElaObject
		{
			private StringBuilder builder;

			internal ElaStringBuilder() : base(ElaTraits.Convert|ElaTraits.Show|ElaTraits.Eq)
			{
				builder = new StringBuilder();
			}


			protected override ElaValue Convert(ObjectType type, ExecutionContext ctx)
			{
				if (type == ObjectType.String)
					return new ElaValue(builder.ToString());
				else
				{
					ctx.ConversionFailed(new ElaValue(this), type, "Only conversion to string is supported by string builder.");
					return Default();
				}
			}


			protected override string Show(ExecutionContext ctx, ShowInfo info)
			{
				return new ElaValue(builder.ToString()).ToString();
			}


			protected override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
			{
				return new ElaValue(left.AsObject() == right.AsObject());
			}


			protected override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
			{
				return new ElaValue(left.AsObject() != right.AsObject());
			}


			internal void Append(string str)
			{
				builder.Append(str);
			}
		}
		#endregion


		#region Methods
		[Function("format")]
		internal string Format(string format, ElaValue arg)
		{
			var obj = default(IEnumerable<ElaValue>);

			if ((obj = (arg.AsObject() as IEnumerable<ElaValue>)) != null)
				return String.Format(format, obj.Select(o => o.AsObject()).ToArray());
			else
				return String.Format(format, arg);
		}


		[Function("builder")]
		internal ElaObject CreateBuilder()
		{
			return new ElaStringBuilder();
		}


		[Function("append")]
		internal ElaObject Append(string str, ElaObject obj)
		{
			var builder = obj as ElaStringBuilder;

			if (builder == null)
				throw new ElaParameterTypeException(obj.GetTypeInfo().TypeCode);

			builder.Append(str);
			return builder;
		}


		[Function("upper")]
		internal string ToUpper(string str)
		{
			return str.ToUpper();
		}


		[Function("lower")]
		internal string ToLower(string str)
		{
			return str.ToLower();
		}


		[Function("indexOf")]
		internal int IndexOf(string sc, string str)
		{
			return str.IndexOf(sc);
		}


		[Function("indexOfFrom")]
		internal int IndexOf2(string sc, int index, string str)
		{
			return str.IndexOf(sc, index);
		}


		[Function("lastIndexOf")]
		internal int LastIndexOf(string sc, string str)
		{
			return str.LastIndexOf(sc);
		}


		[Function("indexOfAny")]
		internal int IndexOfAny(char[] chars, string str)
		{
			return str.IndexOfAny(chars);
		}


		[Function("indexOfAnyFrom")]
		internal int IndexOfAnyFrom(char[] chars, int index, string str)
		{
			return str.IndexOfAny(chars, index);
		}


		[Function("trim")]
		internal string Trim(string str)
		{
			return str.Trim();
		}


		[Function("trimStart")]
		internal string TrimStart(string str)
		{
			return str.TrimStart();
		}


		[Function("trimEnd")]
		internal string TrimEnd(string str)
		{
			return str.TrimEnd();
		}


		[Function("trimChars")]
		internal string TrimChars(string str, char[] chars)
		{
			return str.Trim(chars);
		}


		[Function("trimStartChars")]
		internal string TrimStartChars(string str, char[] chars)
		{
			return str.TrimStart(chars);
		}


		[Function("trimEndChars")]
		internal string TrimEndChars(string str, char[] chars)
		{
			return str.TrimEnd(chars);
		}


		[Function("startsWith")]
		internal bool StartsWith(string str, string search)
		{
			return str.StartsWith(search);
		}
		

		[Function("endsWith")]
		internal bool EndsWith(string str, string search)
		{
			return str.EndsWith(search);
		}


		[Function("replace")]
		internal string Replace(string str, string search, string replace)
		{
			return str.Replace(search, replace);
		}


		[Function("remove")]
		internal string Remove(string str, int start, int count)
		{
			return str.Remove(start, count);
		}


		[Function("split")]
		internal string[] Split(string str, string[] seps)
		{
			return str.Split(seps, StringSplitOptions.RemoveEmptyEntries);
		}



		[Function("substr")]
		internal string Substring(string str, int start, int length)
		{
			return str.Substring(start, length);
		}


		[Function("splitToList")]
		internal ElaList SplitToList(string[] seps, string str)
		{
			var arr = Split(str, seps);
			var list = ElaList.GetNil();

			for (var i = arr.Length - 1; i > -1; i--)
				list = new ElaList(list, new ElaValue(arr[i]));
			
			return list;
		}


		[Function("insert")]
		internal string Insert(string str, int index)
		{
			return str.Insert(index, str);
		}


		[Function("padRight")]
		internal string PadRight(string str, char c, int totalWidth)
		{
			return str.PadRight(totalWidth, c);
		}


		[Function("padLeft")]
		internal string PadLeft(string str, char c, int totalWidth)
		{
			return str.PadLeft(totalWidth, c);
		}
		#endregion
	}
}
