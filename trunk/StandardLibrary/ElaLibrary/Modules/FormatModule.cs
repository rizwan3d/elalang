using System;
using Ela.ModuleGenerator;
using Ela.Runtime;
using System.Text;
using System.Collections.Generic;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary.Modules
{
	public partial class FormatModule
	{
		#region Construction
		public FormatModule()
		{

		}
		#endregion


		#region Nested Types
		private sealed class StringFormat
		{
			#region Construction
			internal StringFormat(List<Char> flags, List<Int32> places, string format)
			{
				Flags = flags;
				Places = places;
				FormatString = format;
			}
			#endregion


			#region Properties
			internal List<Char> Flags { get; private set; }

			internal List<Int32> Places { get; private set; }

			internal string FormatString { get; private set; }
			#endregion
		}


		private class BaseFormatTyper : ElaFunction
		{
			#region Construction
			private bool newLine;

			internal BaseFormatTyper(int args, bool newLine) : base(args)
			{
				Formatters = new Dictionary<Char,ElaFunction>();
				this.newLine = newLine;
			}
			#endregion


			#region Methods
			protected ElaValue CreateFormatter(ElaFunction cont, string format)
			{
				return CreateFormatter(CreateFunc(cont), format);
			}


			protected ElaValue CreateFormatter(Func<String,ElaValue> cont, string format)
			{
				var sf = ParseFormat(newLine ? format + Environment.NewLine : format);

				if (sf.Places.Count == 0)
					return new ElaValue(format);
				else
					return new ElaValue(new FormatterFunction(cont, sf, Formatters));
			}
			#endregion


			#region Properties
			internal Dictionary<Char, ElaFunction> Formatters { get; private set; }
			#endregion
		}


		private sealed class KFormatTyperFunction : BaseFormatTyper
		{
			#region Construction
			internal KFormatTyperFunction(bool newLine) : base(2, newLine)
			{
				
			}
			#endregion


			#region Methods
			public override ElaValue Call(params ElaValue[] args)
			{
				if (args[0].DataType != ObjectType.Function)
					throw new ElaParameterTypeException(ObjectType.Function, args[0].DataType);

				if (args[1].DataType != ObjectType.String)
					throw new ElaParameterTypeException(ObjectType.String, args[1].DataType);

				return CreateFormatter(args[0].AsFunction(), args[1].AsString());
			}
			#endregion
		}


		private sealed class FormatTyperFunction : BaseFormatTyper
		{
			#region Construction
			private Func<String,ElaValue> cont;

			internal FormatTyperFunction(Func<String,ElaValue> cont, bool newLine) : base(1, newLine)
			{
				this.cont = cont;
			}
			#endregion


			#region Methods
			public override ElaValue Call(params ElaValue[] args)
			{
				if (args[0].DataType != ObjectType.String)
					throw new ElaParameterTypeException(ObjectType.String, args[0].DataType);

				return CreateFormatter(cont, args[0].AsString());
			}
			#endregion
		}


		private sealed class FormatterFunction : ElaFunction
		{
			#region Construction
			private Dictionary<Char,ElaFunction> formatters;
			private StringFormat fmt;
			private Func<String,ElaValue> cont;

			internal FormatterFunction(Func<String,ElaValue> cont, StringFormat fmt, Dictionary<Char,ElaFunction> formatters) : base(fmt.Places.Count)
			{
				this.cont = cont;
				this.fmt = fmt;
				this.formatters = formatters;
			}
			#endregion


			#region Methods
			public override ElaValue Call(params ElaValue[] args)
			{
				var shift = 0;
				var sb = new StringBuilder(fmt.FormatString);

				for (var i = 0; i < args.Length; i++)
				{
					var pos = fmt.Places[i];
					var flag = fmt.Flags[i];

					var str = flag == 'a' ? args[i].ToString() : Format(args[i], flag);
					sb.Insert(pos + shift, str);
					shift += str.Length;
				}

				return cont(sb.ToString());
			}


			private string Format(ElaValue val, char flag)
			{
				var fun = default(ElaFunction);

				if (!formatters.TryGetValue(flag, out fun))
					throw new Exception(String.Format("A unknown format flag '{0}'.", flag));

				return fun.Call(val).AsString();
			}
			#endregion
		}
		#endregion


		#region Methods
		partial void AuxInitialize()
		{
			Func<String, ElaValue> print = s => { Console.Write(s); return new ElaValue(ElaUnit.Instance); };
			Add("kprintf", new KFormatTyperFunction(false));
			Add("kprintfn", new KFormatTyperFunction(true));
			Add("sprintf", new FormatTyperFunction(s => new ElaValue(s), false));
			Add("sprintfn", new FormatTyperFunction(s => new ElaValue(s), false));
			Add("printf", new FormatTyperFunction(print, false));
			Add("printfn", new FormatTyperFunction(print, true));
		}


		[Function("regf")]
		internal ElaFunction RegisterFormatter(char flag, ElaFunction formatter, ElaFunction formatFun)
		{
			var ff = formatFun as BaseFormatTyper;

			if (ff == null)
				throw new ElaParameterTypeException(ObjectType.Function);

			var nff = (BaseFormatTyper)ff.Clone();
			nff.Formatters.Remove(flag);
			nff.Formatters.Add(flag, formatter);
			return nff;
		}
		#endregion



		#region Helper Methods
		private static StringFormat ParseFormat(string format)
		{
			var buffer = format.ToCharArray();
			var flag = false;
			var pc = '\0';
			var sb = new StringBuilder();
			var flags = new List<Char>();
			var places = new List<Int32>();

			for (var i = 0; i < buffer.Length; i++)
			{
				var c = buffer[i];

				if (!flag)
				{
					if (c == '%' && pc != '%')
					{
						flag = true;
						flags.Add('a');
						places.Add(sb.Length);
					}
					else
						sb.Append(c);
				}
				else
				{
					if (Char.IsLetterOrDigit(c))
					{
						flag = false;
						flags[flags.Count - 1] = c;
					}
					else
						throw new FormatException();
				}

				pc = c;
			}

			return new StringFormat(flags, places, sb.ToString());
		}


		private static Func<String,ElaValue> CreateFunc(ElaFunction fun)
		{
			return v => fun.Call(new ElaValue(v));
		}
		#endregion
	}
}