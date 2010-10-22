using System;
using System.Linq;
using Ela.ModuleGenerator;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using System.Collections.Generic;

namespace Ela.StandardLibrary.Modules
{
	public partial class StringModule
	{
		#region Construction
		public StringModule()
		{

		}
		#endregion


		#region Methods
		[Function("format", UnlimitedParameters = true)]
		internal string Format(RuntimeValue[] args)
		{
			if (args.Length == 0)
				return String.Empty;
			else if (args[0].DataType != ObjectType.String)
				throw new ElaParameterTypeException(ObjectType.String, args[0].DataType);

			var str = args[0].ToString();

			if (args.Length == 2)
			{
				var obj = default(IEnumerable<RuntimeValue>);

				if ((obj = (args[1].ToObject() as IEnumerable<RuntimeValue>)) != null)
					return String.Format(str, obj.Select(o => o.ToObject()).ToArray());
				else
					return String.Format(str, obj);
			}
			else
			{
				var arr = new object[args.Length - 1];

				for (var i = 1; i < args.Length; i++)
					arr[i - 1] = args[i].ToObject();

				return String.Format(str, arr);
			}
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
		internal int IndexOf(string str, string cs)
		{
			return str.IndexOf(cs);
		}


		[Function("indexOfFrom")]
		internal int IndexOf2(int index, string str)
		{
			return str.IndexOf(str, index);
		}


		[Function("lastIndexOf")]
		internal int LastIndexOf(string str, string cs)
		{
			return str.LastIndexOf(cs);
		}


		[Function("indexOfAny")]
		internal int IndexOfAny(string str, char[] chars)
		{
			return str.IndexOfAny(chars);
		}


		[Function("indexOfAnyFrom")]
		internal int IndexOfAnyFrom(string str, int index, char[] chars)
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
		internal ElaList SplitToList(string str, string[] seps)
		{
			var arr = Split(str, seps);
			var list = ElaList.Nil;

			for (var i = arr.Length - 1; i > -1; i--)
				list = new ElaList(list, new RuntimeValue(arr[i]));
			
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
