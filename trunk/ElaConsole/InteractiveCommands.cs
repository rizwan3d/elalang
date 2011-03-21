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
				case "help":
					PrintHelp();
					break;
				case "clear":
					Console.Clear();
					break;
				case "exit":
					Environment.Exit(0);
					break;
			}
		}


		internal void PrintHelp()
		{
			helper.PrintHelp();
		}
	}
}
