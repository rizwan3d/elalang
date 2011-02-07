using System;
using System.Collections.Generic;
using System.IO;
using Ela.CodeModel;
using Ela.Compilation;
using Ela.Parsing;

namespace Ela.Linking
{
	public sealed class ElaLinker : ElaLinker<ElaParser,ElaCompiler>
	{
		#region Construction
		public ElaLinker(LinkerOptions linkerOptions, CompilerOptions compOptions, FileInfo rootFile) :
			base(linkerOptions, compOptions, rootFile)
		{

		}
		#endregion
	}


	public class ElaLinker<P,C> where P : IElaParser, new() where C : IElaCompiler, new()
	{
		#region Construction
		private const string EXT = ".ela";
		private const string OBJEXT = ".elaobj";
		private const string DLLEXT = ".dll";

		internal static readonly FileInfo MemoryFile = new FileInfo("memory");
		private Dictionary<String,Dictionary<String,ElaModuleAttribute>> foreignModules;
		private FastList<DirectoryInfo> dirs;
		private bool stdLoaded;
		
		public ElaLinker(LinkerOptions linkerOptions, CompilerOptions compOptions, FileInfo rootFile)
		{
			dirs = new FastList<DirectoryInfo>();

			if (linkerOptions.CodeBase.LookupStartupDirectory)
			{
				if (rootFile != null)
					dirs.Add(rootFile.Directory);

				dirs.Add(new DirectoryInfo(Environment.CurrentDirectory));
			}

			dirs.AddRange(linkerOptions.CodeBase.Directories);
			LinkerOptions = linkerOptions;
			CompilerOptions = compOptions;
			RootFile = rootFile;
			Messages = new List<ElaMessage>();
			Assembly = new CodeAssembly();
			Success = true;
			foreignModules = new Dictionary<String,Dictionary<String,ElaModuleAttribute>>();
		}
		#endregion


		#region Methods
		public virtual LinkerResult Build()
		{
			var frame = Build(null, RootFile);
			RegisterFrame(new ModuleReference(
				Path.GetFileNameWithoutExtension(RootFile.Name)), frame, RootFile);
			return new LinkerResult(Assembly, Success, Messages);
		}


		internal void ProcessIncludes(CodeFrame frame)
		{
			foreach (var kv in frame.References)
				ResolveModule(kv.Value);
		}


		internal void ResolveModule(ModuleReference mod)
		{
			LoadStdLib();

			if (!Assembly.IsModuleRegistered(mod.ToString()))
			{
				if (mod.DllName == null && stdLoaded && TryLoadStandardModule(mod))
					return;
				else if (mod.DllName != null)
					ResolveDll(mod);
				else
				{
					var ela = String.Concat(mod.ModuleName, EXT);
					var obj = LinkerOptions.ForceRecompile ? null : String.Concat(mod.ModuleName, OBJEXT);
					bool bin;
					var fi = FindModule(mod, ela, obj, out bin);

					if (fi != null)
					{
						if (!bin)
						{
							var frame = Build(mod, fi);
							RegisterFrame(mod, frame, fi);
						}
						else
						{
							var frame = ReadObjectFile(mod, fi);
							RegisterFrame(mod, frame, fi);
						}
					}
				}
			}
		}


		private CodeFrame ReadObjectFile(ModuleReference mod, FileInfo fi)
		{
			var obj = new ObjectFileReader(fi);
			
			try
			{
				return obj.Read();
			}
			catch (ElaException ex)
			{
				AddError(ElaLinkerError.ObjectFileReadFailed, fi, mod.Line, mod.Column, fi.Name, ex.Message);
				return null;
			}
		}


		private void LoadStdLib()
		{
			if (!stdLoaded && !String.IsNullOrEmpty(LinkerOptions.StandardLibrary))
			{
				var mod = new ModuleReference(null, LinkerOptions.StandardLibrary, null, 0, 0) { IsStandardLibrary = true };
				var fi = default(FileInfo);
				LoadAssemblyFile(mod, out fi);
				stdLoaded = true;
			}
		}


		private bool TryLoadStandardModule(ModuleReference mod)
		{
			var dict = default(Dictionary<String,ElaModuleAttribute>);
			var attr = default(ElaModuleAttribute);

			if (foreignModules.TryGetValue("$", out dict) && dict.TryGetValue(mod.ModuleName, out attr))
			{
				LoadModule(mod, attr, new FileInfo(LinkerOptions.StandardLibrary));
				return true;
			}
			else
				return false;
		}


		internal void RegisterFrame(ModuleReference mod, CodeFrame frame, FileInfo fi)
		{
			if (frame != null)
			{
				frame.File = fi;
				Assembly.AddModule(mod.ToString(), frame);
				ProcessIncludes(frame);
			}
		}


		private void ResolveDll(ModuleReference mod)
		{
			var dict = default(Dictionary<String,ElaModuleAttribute>);
			var fi = default(FileInfo);

			if (foreignModules.TryGetValue(mod.DllName, out dict) || LoadAssemblyFile(mod, out fi))
			{
				if (dict == null)
					dict = foreignModules[mod.DllName];

				var attr = default(ElaModuleAttribute);

				if (!dict.TryGetValue(mod.ModuleName, out attr))
					AddError(ElaLinkerError.ModuleNotFoundInAssembly, new FileInfo(mod.DllName), mod.Line, mod.Column,
						mod.ModuleName, mod.DllName);
				else
					LoadModule(mod, attr, fi);
			}				
		}


		private bool LoadAssemblyFile(ModuleReference mod, out FileInfo fi)
		{
			if (mod.DllName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
			{
				AddError(ElaLinkerError.ModuleNameInvalid, new FileInfo("Undefined"), mod.Line, mod.Column, mod.DllName);
				fi = null;
				return false;
			}
			else
			{
				var dll = Path.HasExtension(mod.DllName) ? mod.DllName : String.Concat(mod.DllName, DLLEXT);
				bool uns;
				fi = FindModule(mod, dll, null, out uns);

				if (fi == null)
					return false;
				else
					return LoadAssembly(mod, fi);
			}
		}


		private bool LoadAssembly(ModuleReference mod, FileInfo file)
		{
			var asm = default(System.Reflection.Assembly);

			try
			{
				asm = System.Reflection.Assembly.LoadFile(file.FullName);
			}
			catch (Exception ex)
			{
				AddError(ElaLinkerError.AssemblyLoad, file, mod.Line, mod.Column, file.Name, ex.Message);
				return false;
			}

			var attrs = asm.GetCustomAttributes(typeof(ElaModuleAttribute), false);

			if (attrs.Length == 0)
			{
				AddError(ElaLinkerError.ForeignModuleDescriptorMissing, file, mod.Line, mod.Column, mod);
				return false;
			}
			else
			{
				var dict = new Dictionary<String,ElaModuleAttribute>();

				foreach (ElaModuleAttribute a in attrs)
				{
					if (dict.ContainsKey(a.ModuleName))
					{
						AddError(ElaLinkerError.DuplicateModuleInAssembly, new FileInfo(mod.DllName), mod.Line, mod.Column,
							a.ModuleName, mod.DllName);
						return false;
					}
					else
						dict.Add(a.ModuleName, a);
				}

				foreignModules.Add(mod.IsStandardLibrary ? "$" : mod.DllName, dict);
			}

			return true;
		}


		private void LoadModule(ModuleReference mod, ElaModuleAttribute attr, FileInfo fi)
		{
			var obj = default(ForeignModule);

			try
			{
				obj = Activator.CreateInstance(attr.ClassType) as ForeignModule;
			}
			catch (Exception ex)
			{
				AddError(ElaLinkerError.ForeignModuleInitFailed, fi, mod.Line, mod.Column, mod, ex.Message);
				return;
			}

			if (obj == null)
				AddError(ElaLinkerError.ForeignModuleInvalidType, fi, mod.Line, mod.Column, mod);
			else
			{
				var frame = default(CodeFrame);

				try
				{
					obj.Initialize();
					frame = obj.Compile();
				}
				catch (Exception ex)
				{
					AddError(ElaLinkerError.ForeignModuleInitFailed, fi, mod.Line, mod.Column, mod, ex.Message);
					return;
				}

				RegisterFrame(mod, frame, fi);
			}
		}


		private CodeFrame Build(ModuleReference mod, FileInfo file)
		{
			return Build(mod, file, null, null, null);
		}


		internal CodeFrame Build(ModuleReference mod, FileInfo file, string source, 
			CodeFrame frame, Scope scope)
		{
			var pRes = Parse(file, source);
			var ret = default(CodeFrame);

			if (pRes.Success)
			{
				var cRes = Compile(file, pRes.Expression, frame, scope);
				ret = cRes.CodeFrame;

				if (cRes.Success)
				{
					if (ret.Symbols != null)
						ret.Symbols.File = file != null ? file : RootFile;

					return ret;
				}
				else
					ret = null;
			}

			if (file != RootFile)
				AddError(ElaLinkerError.ModuleLinkFailed, 
					RootFile,
					mod != null ? mod.Line : 0, 
					mod != null ? mod.Column : 0,
					Path.GetFileNameWithoutExtension(file != null ? file.Name :
						RootFile != null ? RootFile.Name : "<" + MemoryFile.Name + ">"));
			return ret;
		}


		internal CompilerResult Compile(FileInfo file, ElaExpression expr, CodeFrame frame, Scope scope)
		{
			var elac = new C();
			var res = frame != null ? elac.Compile(expr, CompilerOptions, frame, scope) :
				elac.Compile(expr, CompilerOptions);
			AddMessages(res.Messages, file);
			return res;
		}


		internal ParserResult Parse(FileInfo file, string source)
		{
			var elap = new ElaParser();
			var res = source != null ? elap.Parse(source) : elap.Parse(file);
			AddMessages(res.Messages, file);
			return res;
		}


		private FileInfo FindModule(ModuleReference mod, string firstName, string secondName, out bool sec)
		{
			var ret1 = default(FileInfo);
			var ret2 = default(FileInfo);
			sec = false;
			
			foreach (var d in dirs)
			{
				if (secondName != null)
					ret2 = Combine(d, mod.Folder, secondName);

				ret1 = Combine(d, mod.Folder, firstName);

				if (ret2 != null && ret1 != null)
				{
					if (!LinkerOptions.SkipTimeStampCheck && ret2.LastWriteTime < ret1.LastWriteTime)
					{
						AddWarning(ElaLinkerWarning.ObjectFileOutdated, ret1, mod.Line, mod.Column, ret2.Name, ret1.Name);
						return ret1;
					}
					else
					{
						sec = true;
						return ret2;
					}
				}
				else if (ret1 != null)
					return ret1;
			}

			AddError(ElaLinkerError.UnresolvedModule, new FileInfo(firstName), mod.Line, mod.Column, 
				Path.GetFileNameWithoutExtension(firstName));
			return null;
		}


		private FileInfo Combine(DirectoryInfo dir, string folder, string fileName)
		{
			var path = folder != null?
				Path.Combine(Path.Combine(dir.FullName, folder), fileName) :
				Path.Combine(dir.FullName, fileName);

			return File.Exists(path) ? new FileInfo(path) : null;
		}
		#endregion

		
		#region Service Methods
		internal void AddError(ElaLinkerError error, FileInfo file, int line, int col, params object[] args)
		{
			Success = false;

			if (file != null)
				Messages.Add(new ElaMessage(Strings.GetError(error, args),
					MessageType.Error, (Int32)error, line, col) { File = file });
		}


		internal void AddWarning(ElaLinkerWarning warning, FileInfo file, int line, int col, params object[] args)
		{
			if (LinkerOptions.NoWarnings)
				return;
			else if (LinkerOptions.WarningsAsErrors)
				AddError((ElaLinkerError)warning, file, line, col, args);
			else
				Messages.Add(new ElaMessage(Strings.GetWarning(warning, args),
					MessageType.Warning, (Int32)warning, line, col) { File = file });
		}


		internal void AddMessages(IEnumerable<ElaMessage> messages, FileInfo file)
		{
			foreach (var m in messages)
			{
				if (m.Type == MessageType.Error)
					Success = false;

				m.File = file;
				Messages.Add(m);
			}
		}
		#endregion


		#region Properties
		public LinkerOptions LinkerOptions { get; private set; }
		
		public CompilerOptions CompilerOptions { get; private set; }

		public FileInfo RootFile { get; private set; }

		internal List<ElaMessage> Messages { get; private set; }

		internal CodeAssembly Assembly { get; set; }

		internal bool Success { get; set; }
		#endregion
	}
}
