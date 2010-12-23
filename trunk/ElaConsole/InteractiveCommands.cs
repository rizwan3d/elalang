using System;
using Ela;
using Ela.Runtime;
using Ela.Parsing;
using Ela.Compilation;
using Ela.Linking;

namespace ElaConsole
{
	internal sealed class InteractiveCommands
	{
		private ElaMachine machine;
		private MessageHelper helper;

		internal InteractiveCommands(ElaMachine machine, MessageHelper helper)
		{
			this.machine = machine;
			this.helper = helper;
		}


		internal void ProcessCommand(string command)
		{
			var idx = command.IndexOf(' ');
			var arg = default(String);
			var cmd = default(String);

			if (idx > -1)
			{
				arg = command.Substring(idx).Trim(' ');
				cmd = command.Substring(1, idx - 1);
			}
			else
				cmd = command.Substring(1);

			switch (cmd)
			{
				case "reload":
					ReloadModule(arg);
					break;
				case "help":
					PrintHelp();
					break;
				case "clear":
					Console.Clear();
					break;
			}
		}


		internal void ReloadModule(string arg)
		{
			if (machine == null || machine.Assembly == null)
			{
				helper.PrintErrorAlways("No modules are currenly loaded");
				return;
			}

			var frame = machine.Assembly.GetRootModule();
			
			if (!frame.References.ContainsKey(arg))
				helper.PrintErrorAlways("Undefined module alias '{0}'.", arg);
			else
			{
				var modRef = frame.References[arg];

				try
				{
					var oldFrame = machine.Assembly.GetModule(modRef.ToString());
					var newFrame = default(CodeFrame);

					if (oldFrame is IntrinsicFrame)
						newFrame = oldFrame;
					else
					{
						var ep = new ElaParser();
						var res = ep.Parse(oldFrame.File);

						if (!res.Success)
						{
							helper.PrintErrors(res.Messages);
							return;
						}

						var ec = new ElaCompiler();
						var cres = ec.Compile(res.Expression, CompilerOptions.Default);
						helper.PrintErrors(cres.Messages);

						if (!cres.Success)
							return;

						newFrame = cres.CodeFrame;
						newFrame.File = oldFrame.File;

						machine.ReloadModule(modRef, newFrame);
						Console.WriteLine("Module '{0}' successfully reloaded.", arg);
					}
				}
				catch (ElaException ex)
				{
					helper.PrintErrorAlways(ex.Message);
				}
			}
		}


		internal void PrintHelp()
		{
			helper.PrintHelp();
		}
	}
}
