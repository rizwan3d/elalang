using System;
using Ela;
using Ela.Runtime;
using Ela.Parsing;
using Ela.Compilation;
using Ela.Linking;
using ElaConsole.Options;

namespace ElaConsole
{
	internal sealed class InteractiveCommands
	{
		private ElaMachine machine;
		private MessageHelper helper;
        private ElaOptions opts;

		internal InteractiveCommands(ElaMachine machine, MessageHelper helper, ElaOptions opts)
		{
			this.machine = machine;
			this.helper = helper;
            this.opts = opts;
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
				case "help":
					PrintHelp();
					break;
				case "clear":
					Console.Clear();
					break;
				case "exit":
					Environment.Exit(0);
					break;
                case "ml":
                    opts.Multiline = !opts.Multiline;
					Console.WriteLine();
					Console.WriteLine("Multiline mode is {0}.", opts.Multiline ? "on" : "off");
                    break;
				default:
					Console.WriteLine();
					Console.WriteLine("Unrecognized interactive command '{0}'.", cmd);
					break;
			}
		}


		internal void PrintHelp()
		{
			helper.PrintHelp();
		}
	}
}
