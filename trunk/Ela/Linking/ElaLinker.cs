using System;
using System.Collections.Generic;
using System.IO;
using Ela.CodeModel;
using Ela.Compilation;
using Ela.Parsing;
using Ela.Runtime;

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
        internal const string ARG_MODULE = "$___ARGS___";

		internal static readonly string MemoryFile = "memory";
        private static readonly ModuleReference argModuleRef = new ModuleReference("$Args");
		private Dictionary<String,Dictionary<String,ElaModuleAttribute>> foreignModules;
		private FastList<DirectoryInfo> dirs;
		private ArgumentModule argModule;
        private CodeFrame argModuleFrame;
		private bool stdLoaded;
        
		public ElaLinker(LinkerOptions linkerOptions, CompilerOptions compOptions, FileInfo rootFile)
		{
			dirs = new FastList<DirectoryInfo>();
            
			if (linkerOptions.CodeBase.LookupStartupDirectory && rootFile != null)
				dirs.Add(rootFile.Directory);

			dirs.AddRange(linkerOptions.CodeBase.Directories);
			LinkerOptions = linkerOptions;
			CompilerOptions = compOptions;
			RootFile = rootFile;
			Messages = new List<ElaMessage>();
			Assembly = new CodeAssembly();
			Success = true;
			foreignModules = new Dictionary<String,Dictionary<String,ElaModuleAttribute>>();

            var dict = new Dictionary<String,ElaModuleAttribute>();
            dict.Add("lang", new ElaModuleAttribute("lang", typeof(LangModule)));
            foreignModules.Add("$", dict);
		}
		#endregion


		#region Methods
        public virtual void AddArgument(string name, object value)
        {
            if (argModule == null)
                argModule = new ArgumentModule();

			argModule.AddArgument(name, value);
        }


		public virtual LinkerResult Build()
		{
			var mod = new ModuleReference(Path.GetFileNameWithoutExtension(RootFile != null ? RootFile.Name : MemoryFile));
			var frame = Build(mod, RootFile);
			RegisterFrame(new ModuleReference(
				Path.GetFileNameWithoutExtension(RootFile.Name)), frame, RootFile, -1);
			return new LinkerResult(Assembly, Success, Messages);
		}


        public virtual LinkerResult Build(string source)
		{
			var mod = new ModuleReference(Path.GetFileNameWithoutExtension(RootFile != null ? RootFile.Name : MemoryFile));
            var frame = Build(mod, null, source, null, null);
			RegisterFrame(new ModuleReference(
				Path.GetFileNameWithoutExtension(RootFile.Name)), frame, RootFile, -1);
			return new LinkerResult(Assembly, Success, Messages);
		}

		
		internal CodeFrame ResolveModule(ModuleReference mod, ExportVars exportVars)
		{
            var frame = default(CodeFrame);
            var hdl = -1;

            if (mod.ModuleName == ARG_MODULE)
            {
                if (argModule == null)
                    argModule = new ArgumentModule();

                if (argModuleFrame == null)
                    argModuleFrame = argModule.Compile();

                frame = argModuleFrame;
                hdl = Assembly.AddModule(mod.ToString(), argModuleFrame, false, mod.LogicalHandle);
            }
            else
            {
                LoadStdLib();

                if ((frame = Assembly.GetModule(mod.ToString(), out hdl)) == null)
                {
                    if (mod.DllName == null && stdLoaded && (frame = TryLoadStandardModule(mod, out hdl)) != null)
                    {

                    }
                    else if (mod.DllName != null)
                        frame = ResolveDll(mod, out hdl);
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
                                frame = Build(mod, fi);
                                hdl = RegisterFrame(mod, frame, fi, mod.LogicalHandle);
                            }
                            else
                            {
                                frame = ReadObjectFile(mod, fi);
                                hdl = RegisterFrame(mod, frame, fi, mod.LogicalHandle);
                            }
                        }
                        else
                        {
                            frame = TryResolveModule(mod);
                            hdl = RegisterFrame(mod, frame, fi, mod.LogicalHandle);
                        }
                    }
                }
            }
            
            if (mod.Parent != null)
                mod.Parent.HandleMap[mod.LogicalHandle] = hdl;

            return frame != null && !mod.RequireQuailified ? FillExports(frame, exportVars, mod.LogicalHandle) : frame;
		}


        private CodeFrame FillExports(CodeFrame frame, ExportVars exportVars, int logicHandle)
        {
            foreach (var kv in frame.GlobalScope.EnumerateVars())
            {
                var sv = kv.Value;

                if ((sv.Flags & ElaVariableFlags.Private) != ElaVariableFlags.Private)
                    exportVars.AddName(kv.Key, 
                        (sv.Flags & ElaVariableFlags.Builtin) == ElaVariableFlags.Builtin ? (ElaBuiltinKind)sv.Data : ElaBuiltinKind.None,
                        logicHandle, sv.Address);
            }

            return frame;
        }


        private void CheckBinaryConsistency(CodeFrame obj, ModuleReference mod, FileInfo fi, ExportVars exportVars)
        {
            foreach (var l in obj.LateBounds)
            {
                var vk = default(ExportVarData);
                var found = exportVars.FindName(l.Name, out vk);

                if (!found || l.Address >> 8 != vk.Address || (l.Address & Byte.MaxValue) != vk.ModuleHandle)
                    AddError(ElaLinkerError.ExportedNameRemoved, fi,
                        mod != null ? mod.Line : 0,
                        mod != null ? mod.Column : 0,
                        l.Name);

                if (found &&
                        (
                        (vk.Kind != ElaBuiltinKind.None && (Int32)vk.Kind != l.Data && l.Data != -1)
                        )
                    )
                    AddError(ElaLinkerError.ExportedNameChanged, fi,
                        mod != null ? mod.Line : 0,
                        mod != null ? mod.Column : 0,
                        l.Name);
            }
        }


		private CodeFrame ReadObjectFile(ModuleReference mod, FileInfo fi)
		{
			var obj = new ObjectFileReader(fi);
			
			try
			{
				var frame = obj.Read();
                frame.HandleMap = new FastList<Int32>(frame.References.Count);
                var exportVars = CreateExportVars(fi);

                foreach (var r in frame.References)
                {
                    if (!mod.NoPrelude || CompilerOptions.Prelude != r.Value.ModuleName)
                        ResolveModule(r.Value, exportVars);
                }

                CheckBinaryConsistency(frame, mod, fi, exportVars);
				return frame;
			}
			catch (ElaException ex)
			{
				AddError(ElaLinkerError.ObjectFileReadFailed, fi, 
                    mod != null ? mod.Line : 0, 
                    mod != null ? mod.Column : 0, 
                    fi.Name, ex.Message);
				return null;
			}
		}


		private void LoadStdLib()
		{
			if (!stdLoaded && !String.IsNullOrEmpty(LinkerOptions.StandardLibrary))
			{
				var mod = new ModuleReference(null, null, LinkerOptions.StandardLibrary, null, 0, 0, false, 0) { IsStandardLibrary = true };
				var fi = default(FileInfo);
				LoadAssemblyFile(mod, out fi);
				stdLoaded = true;
			}
		}


		private CodeFrame TryLoadStandardModule(ModuleReference mod, out int hdl)
		{
			var dict = default(Dictionary<String,ElaModuleAttribute>);
			var attr = default(ElaModuleAttribute);
            hdl = -1;

			if (foreignModules.TryGetValue("$", out dict) && dict.TryGetValue(mod.ModuleName, out attr))
				return LoadModule(mod, attr, new FileInfo(LinkerOptions.StandardLibrary), out hdl);
			else
				return null;
		}


		internal int RegisterFrame(ModuleReference mod, CodeFrame frame, FileInfo fi, int logicHandle)
		{
            if (frame != null)
            {
                frame.File = fi;
                return Assembly.AddModule(mod.ToString(), frame, mod.RequireQuailified, logicHandle);
            }
            else
                return -1;
		}


        protected virtual ExportVars CreateExportVars(FileInfo fi)
        {
            return new ExportVars();
        }


		private CodeFrame ResolveDll(ModuleReference mod, out int hdl)
		{
			var dict = default(Dictionary<String,ElaModuleAttribute>);
			var fi = default(FileInfo);
            hdl = -1;

			if (foreignModules.TryGetValue(mod.DllName, out dict) || LoadAssemblyFile(mod, out fi))
			{
				if (dict == null)
					dict = foreignModules[mod.DllName];

				var attr = default(ElaModuleAttribute);

				if (!dict.TryGetValue(mod.ModuleName, out attr))
					AddError(ElaLinkerError.ModuleNotFoundInAssembly, new FileInfo(mod.DllName), mod.Line, mod.Column,
						mod.ModuleName, mod.DllName);
				else
					return LoadModule(mod, attr, fi, out hdl);
			}

			return null;
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
				{
					UnresolvedModule(mod);
					return false;
				}
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
                var dict = mod.IsStandardLibrary ? foreignModules["$"] : new Dictionary<String,ElaModuleAttribute>();

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

                if (!mod.IsStandardLibrary)
				    foreignModules.Add(mod.DllName, dict);
			}

			return true;
		}


		private CodeFrame LoadModule(ModuleReference mod, ElaModuleAttribute attr, FileInfo fi, out int hdl)
		{
			var obj = default(ForeignModule);
            hdl = -1;

			try
			{
				obj = Activator.CreateInstance(attr.ClassType) as ForeignModule;
			}
			catch (Exception ex)
			{
				AddError(ElaLinkerError.ForeignModuleInitFailed, fi, mod.Line, mod.Column, mod, ex.Message);
				return null;
			}

			if (obj == null)
			{
				AddError(ElaLinkerError.ForeignModuleInvalidType, fi, mod.Line, mod.Column, mod);
				return null;
			}
			else
			{
				var frame = default(CodeFrame);

				try
				{
					obj.Initialize();
					Assembly.RegisterForeignModule(obj);
					frame = obj.Compile();
				}
				catch (Exception ex)
				{
					AddError(ElaLinkerError.ForeignModuleInitFailed, fi, mod.Line, mod.Column, mod, ex.Message);
					return null;
				}

				hdl = RegisterFrame(mod, frame, fi, mod.LogicalHandle);
				return frame;
			}
		}


		private CodeFrame Build(ModuleReference mod, FileInfo file)
		{
			return Build(mod, file, null, null, null);
		}


		internal CodeFrame Build(ModuleReference mod, FileInfo file, string source, 
			CodeFrame frame, Scope scope)
		{
			if (Assembly.ModuleCount == 0)
				Assembly.AddModule(mod.ToString(), frame, mod.RequireQuailified, mod.LogicalHandle);
			
            var ret = default(CodeFrame);
           
            if (file != null && file == RootFile)
            {
                if (!CheckRootFile(out ret))
                    return null;
                else if (ret != null)
                    return ret;

                file = RootFile;
            }

			var pRes = Parse(file, source);
			
			if (pRes.Success)
			{
				var cRes = Compile(file, pRes.Expression, frame, scope, mod.NoPrelude);
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
						RootFile != null ? RootFile.Name : "<" + MemoryFile + ">"));
			return ret;
		}


        internal bool CheckRootFile(out CodeFrame frame)
        {
            frame = null;
            var fnWex = Path.GetFileNameWithoutExtension(RootFile.FullName);
            
            var fnObj = new FileInfo(Path.Combine(RootFile.DirectoryName, fnWex + OBJEXT));
            var fnSrc = new FileInfo(Path.Combine(RootFile.DirectoryName, fnWex + EXT));
            RootFile = fnSrc;

            if (fnObj.Exists && (fnSrc.Exists && fnSrc.LastWriteTime <= fnObj.LastWriteTime) ||
                !fnSrc.Exists)
            {
                frame = ReadObjectFile(null, fnObj);
                return true;
            }
            
            if (!fnSrc.Exists && !fnObj.Exists)
            {
                AddError(ElaLinkerError.UnresolvedModule, RootFile, 0, 0);
                return false;
            }

            return true;
        }


		internal CompilerResult Compile(FileInfo file, ElaExpression expr, CodeFrame frame, Scope scope, bool noPrelude)
		{
			var elac = new C();
			var opts = CompilerOptions;

			if (noPrelude)
			{
				opts = opts.Clone();
				opts.Prelude = null;
			}

            var exportVars = CreateExportVars(file);
			elac.ModuleInclude += (o, e) => ResolveModule(e.Module, exportVars);
			var res = frame != null ? elac.Compile(expr, CompilerOptions, exportVars, frame, scope) :
				elac.Compile(expr, opts, exportVars);
			AddMessages(res.Messages, file == null ? RootFile : file);
			return res;
		}


		internal ParserResult Parse(FileInfo file, string source)
		{
			var elap = new ElaParser();
			var res = source != null ? elap.Parse(source) : elap.Parse(file);
			AddMessages(res.Messages, file == null ? RootFile : null);
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
					ret2 = Combine(d, mod.Path, secondName);

				ret1 = Combine(d, mod.Path, firstName);

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
				else if (ret2 != null)
				{
					sec = true;
					return ret2;
				}
			}

			return null;
		}


		private CodeFrame TryResolveModule(ModuleReference mod)
		{
			var e = new ModuleEventArgs(mod);
			OnModuleResolve(e);

			if (e.HasModule)
				return e.GetFrame();

			UnresolvedModule(mod);
			return null;
		}


		private void UnresolvedModule(ModuleReference mod)
		{
			AddError(ElaLinkerError.UnresolvedModule, new FileInfo(mod.ToString()), mod.Line, mod.Column,
				Path.GetFileNameWithoutExtension(mod.ToString()));
		}


		private FileInfo Combine(DirectoryInfo dir, string[] path, string fileName)
		{
			var finPath = path.Length > 0 ?
				Path.Combine(Path.Combine(dir.FullName, String.Join(Path.DirectorySeparatorChar.ToString(), path)), fileName) :
				Path.Combine(dir.FullName, fileName);

			return File.Exists(finPath) ? new FileInfo(finPath) : null;
		}
		#endregion

		
		#region Service Methods
		internal void AddError(ElaLinkerError error, FileInfo file, int line, int col, params object[] args)
		{
			Success = false;

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


		#region Events
		public event EventHandler<ModuleEventArgs> ModuleResolve;
		protected virtual void OnModuleResolve(ModuleEventArgs e)
		{
			var h = ModuleResolve;

			if (h != null)
				h(this, e);
		}
		#endregion
	}
}
