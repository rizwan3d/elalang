using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Ela;
using Ela.Compilation;
using Ela.Debug;
using Ela.Linking;
using Ela.Parsing;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
using ElaConsole.Options;
using System.Reflection;

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
		private static StringBuilder codeLines;
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

			try
			{
				if (String.IsNullOrEmpty(opt.FileName))
				{
					helper.PrintInteractiveModeLogo();
					StartInteractiveMode();
					return R_OK;
				}

				if (opt.Compile)
				{
					if (!CheckExists(opt.FileName))
					{
						helper.PrintError("File '{0}' doesn't exist.", opt.FileName);
						return R_ERR;
					}

					return Compile();
				}
				else
					return InterpretFile();
			}
			finally
			{
				if (vm != null)
					vm.Dispose();
			}
		}


        private static bool CheckExists(string fileName)
        {
            var fi = new FileInfo(fileName);

            if (!fi.Exists)
            {
                var fext = Path.GetFileName(fileName);

                if (fext.ToUpper().EndsWith(".ELA"))
                    return false;
                else
                {
                    opt.FileName = Path.Combine(fi.DirectoryName,
                        Path.GetFileNameWithoutExtension(fileName) + ".ela");
                    return CheckExists(opt.FileName);
                }
            }
            else
                return true;
        }


		private static int Compile()
		{
			var frame = default(CodeFrame);

			try
			{
				var el = new ElaLinker(CreateLinkerOptions(), CreateCompilerOptions(), new FileInfo(opt.FileName));
				var res = el.Build();
				helper.PrintErrors(res.Messages);

				if (!res.Success)
					return R_ERR;

				frame = res.Assembly.GetRootModule();
			}
			catch (ElaException ex)
			{
				helper.PrintInternalError(ex); 
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
                    helper.PrintUnableWriteFile(opt.OutputFile, ex);
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
					helper.PrintUnableWriteFile(opt.OutputFile, ex);
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
				helper.PrintInternalError(ex);
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
                helper.PrintInvalidOption(ex);
				return false;
			}
		}


		private static void StartInteractiveMode()
		{
			codeLines = new StringBuilder();
			helper.PrintPrompt();
            var app = String.Empty;
				
			for (;;)
			{
                var source = app + Console.ReadLine();

				if (!String.IsNullOrEmpty(source))
				{
					source = source.Trim('\0');

					if (source.Length > 0)
					{
						if (source[0] == '#')
						{
							var cmd = new InteractiveCommands(vm, helper, opt);
							cmd.ProcessCommand(source);
							helper.PrintPrompt();
                            app = String.Empty;
						}
						else
						{
							if (!opt.Multiline)
							{
                                Console.WriteLine();
                                InterpretString(source);
                                helper.PrintPrompt();
                                app = String.Empty;
							}
							else
							{
								if (source.Length >= 2 &&
									source[source.Length - 1] == ';' &&
									source[source.Length - 2] == ';')
								{
									codeLines.AppendLine(source.TrimEnd(';'));
									Console.WriteLine();
                                    InterpretString(codeLines.ToString());
									codeLines = new StringBuilder();
									helper.PrintPrompt();
                                    app = String.Empty;
								}
								else
								{
									codeLines.AppendLine(source);
									helper.PrintSecondaryPrompt();

                                    var indent = IndentHelper.GetIndent(source);
                                    app = new String(' ', indent);
                                    Console.Write(app);
								}
							}
						}
					}
				}
			}
		}


		private static int InterpretFile()
		{
			var res = default(LinkerResult);			
			
			try 
			{
				linker = new ElaIncrementalLinker(CreateLinkerOptions(), CreateCompilerOptions(),
					new FileInfo(opt.FileName));
                
                if (opt.Arguments.Count > 0)
                {
                    var tup = CompileArguments();
                    linker.AddArgument("args", tup);
                }
                        
				res = linker.Build();
			}
			catch (ElaException ex)
			{
				helper.PrintInternalError(ex); 
				return R_ERR;
			}

			helper.PrintErrors(res.Messages);
			
			if (!res.Success)
				return R_ERR;
			else
			{
				var ret = Execute(res.Assembly, false);

				if (ret == R_OK && opt.LunchInteractive)
					StartInteractiveMode();
				
				return ret;
			}
		}


		private static int InterpretString(string source)
		{
            if (linker == null)
            {
                linker = new ElaIncrementalLinker(CreateLinkerOptions(), CreateCompilerOptions());

                if (opt.Arguments.Count > 0)
                {
                    var tup = CompileArguments();
                    linker.AddArgument("args", tup);
                }
            }

			linker.SetSource(source);
			var res = linker.Build();
			helper.PrintErrors(res.Messages);

            if (!res.Success)
            {
                if (res.Assembly != null)
                {
                    var r = res.Assembly.GetRootModule();

                    if (r != null)
                        lastOffset = r.Ops.Count;
                }

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
				var gen = new EilGenerator(mod);
				Console.WriteLine("EIL ({0}-{1}):", lastOffset, mod.Ops.Count - 1);
				Console.Write(gen.Generate(lastOffset));
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
                        vm = new ElaMachine(asm);
                    else
                    {
                        vm.RefreshState();
                        vm.Recover();
                    }

					if (opt.ShowTime && !interactive)
						Warmup(asm); //GIT
					
					var os = lastOffset;
					lastOffset = mod.Ops.Count;
                    var sw = new Stopwatch();
                    sw.Start();
					var exer = vm.Run(os);
                    sw.Stop();

					if (opt.ShowTime && !interactive)
						Console.WriteLine("Execution time: {0}", sw.Elapsed);

					if (exer.ReturnValue.TypeCode != ElaTypeCode.None && exer.ReturnValue.TypeCode != ElaTypeCode.Unit)
						Console.WriteLine(exer.ReturnValue.ToString());
				}
				catch (ElaCodeException ex)
				{
					helper.PrintError(ex.ToString());
					return R_ERR;
				}
			}

			return R_OK;
		}


		private static ElaTuple CompileArguments()
		{
			var arr = new ElaValue[opt.Arguments.Count];

			for (var i = 0; i < opt.Arguments.Count; i++)
				arr[i] = new ElaValue(opt.Arguments[i]);

			return new ElaTuple(arr);
		}


		private static CompilerOptions CreateCompilerOptions()
		{
			return new CompilerOptions 
				{
					WarningsAsErrors = opt.WarningsAsErrors,
					NoWarnings = opt.NoWarnings,
					ShowHints = !opt.SupressHints,
					GenerateDebugInfo = opt.Debug,
					Optimize = !opt.DisableOptimization,
					Prelude = opt.Prelude
				};
		}


		private static LinkerOptions CreateLinkerOptions()
		{
			var linkOpt = new LinkerOptions 
				{ 
					ForceRecompile = opt.ForceRecompile,
					SkipTimeStampCheck = opt.SkipTimeStampCheck,
					WarningsAsErrors = opt.LinkerWarningsAsErrors,
					NoWarnings = opt.LinkerNoWarnings,
					StandardLibrary = opt.StandardLibrary
				}; 
			linkOpt.CodeBase.LookupStartupDirectory = !opt.IgnoreStartupDirectory;

			foreach (var s in opt.ReferencePaths)
				linkOpt.CodeBase.Directories.Add(new DirectoryInfo(s));

			return linkOpt;
		}


		private static void Warmup(CodeAssembly asm)//GIT
		{
			helper.PrintExecuteFirstTime();
			new ElaMachine(asm).Run();
			helper.PrintExecuteSecondTime();
		}
		#endregion
	}
}