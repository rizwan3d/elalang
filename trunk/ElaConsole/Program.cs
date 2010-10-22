using System;
using System.IO;
using Ela;
using Ela.Compilation;
using Ela.Debug;
using Ela.Linking;
using Ela.Parsing;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
using ElaConsole.Options;

namespace ElaConsole
{
	internal static class Program
	{
		#region Construction
		private const int R_OK = 0;
		private const int R_ERR = -1;

		private static ElaOptions opt;
		private static ElaIncrementalLinker linker;
		private static ElaMachine vm;
		private static int lastOffset;
		private static MessageHelper helper;
		#endregion


		#region Methods
		private static int Main(string[] args) 
		{
			if (!ReadOptions(args))
				return R_ERR;

			helper = new MessageHelper(opt);
			helper.PrintLogo();
		
			if (!helper.ValidateOptions())
				return R_ERR;

			if (opt.ShowHelp)
			{
				helper.PrintHelp();
				return R_OK;
			}

			if (String.IsNullOrEmpty(opt.FileName))
			{
				helper.PrintInteractiveModeLogo();
				StartInteractiveMode();
			}
			else if (!File.Exists(opt.FileName))
			{
				helper.PrintError("File '{0}' doesn't exist.", opt.FileName);
				return R_ERR;
			}
			else
			{
				if (opt.Compile)
					return Compile();
				else
					return InterpretFile();
			}

			return R_OK;
		}


		private static int Compile()
		{
			var frame = default(CodeFrame);

			try
			{
				var ep = new ElaParser();
				var res = ep.Parse(new FileInfo(opt.FileName));
				helper.PrintErrors(res.Messages);

				if (!res.Success)
					return R_ERR;

				var ec = new ElaCompiler();
				var cres = ec.Compile(res.CodeUnit, CreateCompilerOptions());
				helper.PrintErrors(cres.Messages);

				if (!cres.Success)
					return R_ERR;

				frame = cres.CodeFrame;
			}
			catch (ElaException ex)
			{
				helper.PrintError("Internal error: {0}", ex.Message);
				return R_ERR;
			}

			var fi = default(FileInfo);

			if (!String.IsNullOrEmpty(opt.OutputFile))
			{
				try
				{
					fi = new FileInfo(opt.OutputFile);
				}
				catch (Exception ex)
				{
					helper.PrintError("Unable to write to the file {0}. Error: {1}", 
						opt.OutputFile, ex.Message);
					return R_ERR;
				}
			}
			else
				fi = new FileInfo(Path.ChangeExtension(opt.FileName, ".elaobj"));

			if (!fi.Exists)
			{
				try
				{
					fi.Delete();
				}
				catch (Exception ex)
				{
					helper.PrintError("Unable to write to the file {0}. Error: {1}",
						opt.OutputFile, ex.Message);
					return R_ERR;
				}
			}

			var obj = new ObjectFileWriter(fi);

			try
			{
				obj.Write(frame);
			}
			catch (ElaException ex)
			{
				helper.PrintError("Internal compilation error: {0}", ex.Message);
				return R_ERR;
			}

			Console.WriteLine("Compilation completed. File '{0}' created.", fi.FullName);
			return R_OK;
		}


		private static bool ReadOptions(string[] args)
		{
			try
			{
				opt = new ElaOptions();
				ConfigReader.ReadOptions(opt);
				var clp = new CommandLineParser(opt);
				clp.Parse(args);
				return true;
			}
			catch (ElaOptionException ex)
			{
				helper = new MessageHelper(null);

				switch (ex.Error)
				{
					case ElaOptionError.InvalidFormat:
						if (!String.IsNullOrEmpty(ex.Option))
							helper.PrintErrorAlways("Invalid format for the '{0}' option.", ex.Option);
						else
							helper.PrintErrorAlways("Invalid command line format.");
						break;
					case ElaOptionError.UnknownOption:
						helper.PrintErrorAlways("Unknown command line option '{0}'.", ex.Option);
						break;
				}
					
				return false;
			}
		}


		private static void StartInteractiveMode()
		{
			for (;;)
			{
				helper.PrintPrompt();
				var source = Console.ReadLine();

				if (!opt.Silent)
					Console.WriteLine();

				if (!String.IsNullOrEmpty(source))
				{
					source = source.Trim('\0');
					InterpretString(source);
				}

				if (opt.OneCommand)
					return;
			}
		}


		private static int InterpretFile()
		{
			var source = String.Empty;
			var res = default(LinkerResult);
			
			if (opt.ShowBuildTime) //GIT
			{
				var l = new ElaIncrementalLinker(new LinkerOptions(), CompilerOptions.Default);
				l.SetSource("var x = 0");
				l.Build();
			}
			
			var dt = DateTime.Now;
			
			try 
			{
				linker = new ElaIncrementalLinker(CreateLinkerOptions(), CreateCompilerOptions(),
					new FileInfo(opt.FileName));
				res = linker.Build();
			}
			catch (ElaException ex)
			{
				helper.PrintError("Internal error: {0}", ex.Message);
				return R_ERR;
			}

			helper.PrintErrors(res.Messages);
			
			if (!res.Success)
				return R_ERR;
			else
			{
				if (opt.ShowBuildTime)
					Console.WriteLine("Build taken: {0}", DateTime.Now - dt);

				var ret = Execute(res.Assembly, false);

				if (ret == R_OK && opt.LunchInteractive)
					StartInteractiveMode();
				
				return ret;
			}
		}


		private static int InterpretString(string source)
		{
			if (linker == null)
				linker = new ElaIncrementalLinker(CreateLinkerOptions(), CreateCompilerOptions());

			linker.SetSource(source);
			var res = linker.Build();

			if (!res.Success)
			{
				helper.PrintErrors(res.Messages);
				return R_ERR;
			}
			else
				return Execute(res.Assembly, true);
		}


		private static int Execute(CodeAssembly asm, bool interactive)
		{
			var mod = asm.GetRootModule();

			if (opt.ShowEil)
			{
				if (!opt.ShowBuildTime)
				{
					var gen = new EilGenerator(mod);
					Console.WriteLine("EIL ({0}-{1}):", lastOffset, mod.Ops.Count - 1);
					Console.Write(gen.Generate(lastOffset));
				}

				lastOffset = mod.Ops.Count;
			}
			else if (opt.ShowSymbols != SymTables.None)
			{
				var gen = new DebugReader(mod.Symbols);
				helper.PrintSymTables(gen);
				lastOffset = mod.Ops.Count;
			}
			else
			{
				try
				{
					if (vm == null)
					{
						if (opt.Arguments.Count > 0)
						{
							var arr = CompileArguments();
							asm.AddArgument("args", arr);
						}

						vm = new ElaMachine(asm);
					}
					else
						vm.RefreshState();

					if (opt.ShowTime && !interactive)
						new ElaMachine(CodeFrame.Empty).Run(); //GIT
					
					var os = lastOffset;
					lastOffset = mod.Ops.Count;
					var dt = DateTime.Now;
					var exer = vm.Run(os);

					if (opt.ShowTime && !interactive)
						Console.WriteLine("Execution time: {0}", DateTime.Now - dt);

					if (exer.ReturnValue.DataType != ObjectType.None && exer.ReturnValue.DataType != ObjectType.Unit)
						Console.WriteLine(ValueFormatter.FormatValue(exer.ReturnValue));
				}
				catch (ElaCodeException ex)
				{
					Console.WriteLine(ex.ToString());
					return R_ERR;
				}
			}

			return R_OK;
		}


		private static ElaArray CompileArguments()
		{
			var arr = new ElaArray(opt.Arguments.Count);

			for (var i = 0; i < opt.Arguments.Count; i++)
				arr[i] = new RuntimeValue(opt.Arguments[i]);

			return arr;
		}


		private static CompilerOptions CreateCompilerOptions()
		{
			return new CompilerOptions {
				WarningsAsErrors = opt.WarningsAsErrors,
				NoWarnings = opt.NoWarnings,
				ShowHints = !opt.SupressHints,
				StrictMode = opt.StrictMode,
				GenerateDebugInfo = opt.Debug,
				Optimize = !opt.DisableOptimization
			};
		}


		private static LinkerOptions CreateLinkerOptions()
		{
			var linkOpt = new LinkerOptions { 
				ForceRecompile = opt.ForceRecompile,
				SkipTimeStampCheck = opt.SkipTimeStampCheck,
				WarningsAsErrors = opt.LinkerWarningsAsErrors,
				NoWarnings = opt.LinkerNoWarnings
			}; 
			linkOpt.CodeBase.LookupStartupDirectory = !opt.IgnoreStartupDirectory;

			foreach (var s in opt.ReferencePaths)
				linkOpt.CodeBase.Directories.Add(new DirectoryInfo(s));

			return linkOpt;
		}
		#endregion
	}
}