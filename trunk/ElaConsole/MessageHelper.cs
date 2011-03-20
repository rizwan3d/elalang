using System;
using System.IO;
using Ela.Parsing;
using ElaConsole.Options;
using Ela;
using System.Collections.Generic;
using Ela.Debug;

namespace ElaConsole
{
	internal sealed class MessageHelper
	{
		#region Construction
		private ElaOptions opt;

		internal MessageHelper(ElaOptions opt)
		{
			this.opt = opt;
		}
		#endregion


		#region Methods
		internal bool ValidateOptions()
		{
			if (opt.ShowSymbols != SymTables.None)
				opt.Debug = true;

			if (opt.Silent && opt.ShowEil)
			{
				PrintErrorAlways("Unable to use -silent option and -eil option together.");
				return false;
			}
			else if (opt.Silent && opt.ShowHelp)
			{
				PrintErrorAlways("Unable to use -silent option and -help option together.");
				return false;
			}
			else if (opt.ShowTime && String.IsNullOrEmpty(opt.FileName))
			{
				PrintErrorAlways("Unable to use -time option in interactive mode.");
				return false;
			}
			else if (opt.LunchInteractive && String.IsNullOrEmpty(opt.FileName))
			{
				PrintErrorAlways("Unable to use -inter option when a file name is not specified.");
				return false;
			}
			else if (opt.Compile && String.IsNullOrEmpty(opt.FileName))
			{
				PrintErrorAlways("Unable to use -compile option when a file name is not specified.");
				return false;
			}
			else if (opt.Compile && opt.LunchInteractive)
			{
				PrintErrorAlways("Unable to use -compile option and -inter option together.");
				return false;
			}
			else if (opt.WarningsAsErrors && opt.NoWarnings)
			{
				PrintErrorAlways("Unable to use -warnaserr option and -nowarn option together.");
				return false;
			}
			else if (opt.LinkerWarningsAsErrors && opt.LinkerNoWarnings)
			{
				PrintErrorAlways("Unable to use -linkWarnaserr option and -linkNowarn option together.");
				return false;
			}

			return true;
		}
		

		internal void PrintLogo()
		{
			if (!opt.NoLogo && !opt.Silent)
			{
				var vr = String.Empty;
				var mono = Type.GetType("Mono.Runtime") != null;
				var rt = mono ? "Mono" : "CLR";

				if (mono)
				{
					var type = Type.GetType("Consts");
					var fi = default(System.Reflection.FieldInfo);

					if (type != null && (fi = type.GetField("MonoVersion")) != null)
						vr = fi.GetValue(null).ToString();
					else
						vr = Environment.Version.ToString();
				}
				else
					vr = Environment.Version.ToString();

                var bit = IntPtr.Size == 4 ? "32" : "64";
				Console.WriteLine("Ela Interpreter version {0}", typeof(ElaParser).Assembly.GetName().Version);
				Console.WriteLine("Running {0} {1} {2}-bit ({3})", rt, vr, bit, Environment.OSVersion);
				Console.WriteLine();
			}
		}


		internal void PrintInteractiveModeLogo()
		{
			if (!opt.NoLogo && !opt.Silent)
			{
				Console.WriteLine("Interactive mode");
				Console.WriteLine("Enter expressions and press <Return> to execute.");
				Console.WriteLine();
			}

		}


		internal void PrintPrompt()
		{
			if (!opt.Silent)
			{
				Console.WriteLine();
				var prompt = String.IsNullOrEmpty(opt.Prompt) ? "ela" : opt.Prompt;
				Console.Write(prompt + ">");
			}
		}


		internal void PrintSecondaryPrompt()
		{
			if (!opt.Silent)
			{
				var promptLength = String.IsNullOrEmpty(opt.Prompt) ? 3 : opt.Prompt.Length;
				Console.Write(new String(' ', promptLength) + ">");
			}
		}


		internal void PrintHelp()
		{
			using (var sr = new StreamReader(typeof(MessageHelper).Assembly.
				GetManifestResourceStream("ElaConsole.Properties.Help.txt")))
				Console.WriteLine(sr.ReadToEnd());
		}


		internal void PrintSymTables(DebugReader gen)
		{
			var prev = false;

			if ((opt.ShowSymbols & SymTables.Lines) == SymTables.Lines)
			{
				Console.WriteLine("Lines:\r\n");
				Console.Write(gen.PrintSymTables(SymTables.Lines));
				prev = true;
			}

			if ((opt.ShowSymbols & SymTables.Scopes) == SymTables.Scopes)
			{
				Console.WriteLine((prev ? "\r\n" : String.Empty) + "Scopes:\r\n");
				Console.Write(gen.PrintSymTables(SymTables.Scopes));
			}

			if ((opt.ShowSymbols & SymTables.Vars) == SymTables.Vars)
			{
				Console.WriteLine((prev ? "\r\n" : String.Empty) + "Variables:\r\n");
				Console.Write(gen.PrintSymTables(SymTables.Vars));
			}

			if ((opt.ShowSymbols & SymTables.Functions) == SymTables.Functions)
			{
				Console.WriteLine((prev ? "\r\n" : String.Empty) + "Functions:\r\n");
				Console.Write(gen.PrintSymTables(SymTables.Functions));
			}
		}


		internal void PrintErrors(IEnumerable<ElaMessage> errors)
		{
			if (!opt.Silent)
			{
				foreach (var e in errors)
					WriteMessage(e.ToString(), e.Type);
			}
		}


		internal void PrintError(string message, params object[] args)
		{
			if (!opt.Silent)
			{
				if (args != null && args.Length > 0)
					WriteMessage(String.Format(message, args), MessageType.Error);
				else
					WriteMessage(message, MessageType.Error);
			}
		}


		internal void PrintErrorAlways(string message, params object[] args)
		{
			WriteMessage(String.Format("Ela Console error: " + message, args), MessageType.Error);
		}


		private void WriteMessage(string msg, MessageType type)
		{
			Console.WriteLine(msg);
		}
		#endregion
	}
}
