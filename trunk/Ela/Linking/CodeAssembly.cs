using System;
using System.Collections.Generic;
using Ela.Compilation;
using Ela.Runtime;

namespace Ela.Linking
{
	public sealed class CodeAssembly 
	{
		#region Construction
		private const string MAIN_NAME = "[Main]";
		private const int MAIN_HDL = 0;
		
		private Dictionary<String,Int32> moduleMap;
		private FastList<CodeFrame> modules;
		private FastList<ForeignModule> foreignModules;
		private Dictionary<String,ElaValue> arguments;
		
		public static readonly CodeAssembly Empty = new CodeAssembly(CodeFrame.Empty);

		public CodeAssembly(CodeFrame frame) : this()
		{
			moduleMap.Add(MAIN_NAME, modules.Count);
			modules.Add(frame);			
		}


		internal CodeAssembly()
		{
			modules = new FastList<CodeFrame>();
			foreignModules = new FastList<ForeignModule>();
			moduleMap = new Dictionary<String,Int32>();
			arguments = new Dictionary<String,ElaValue>();
		}
		#endregion


		#region Methods
		public bool AddModule(string name, ForeignModule module)
		{
			var frame = module.Compile();
			foreignModules.Add(module);
			return AddModule(name, frame);
		}



		internal void RegisterForeignModule(ForeignModule module)
		{
			foreignModules.Add(module);
		}


		public bool AddModule(string name, CodeFrame module)
		{
			if (!moduleMap.ContainsKey(name))
			{
				moduleMap.Add(name, modules.Count);
				modules.Add(module);
				return true;
			}
			else
				return false;
		}


		public bool IsModuleRegistered(string name)
		{
			return moduleMap.ContainsKey(name);
		}


		public CodeFrame GetRootModule()
		{
			return modules[MAIN_HDL];
		}


		public CodeFrame GetModule(string name)
		{
			var hdl = 0;

			if (moduleMap.TryGetValue(name, out hdl))
				return GetModule(hdl);
			else
				return null;
		}


		public CodeFrame GetModule(int handle)
		{
			return modules[handle];
		}


		public int GetModuleHandle(string name)
		{
			return moduleMap[name];
		}


		public string GetModuleName(int handle)
		{
			foreach (var kv in moduleMap)
                if (kv.Value == handle)
                    return kv.Key;

            return null;
		}


		public bool AddArgument(string name, object value)
		{
			if (!arguments.ContainsKey(name))
			{
				var val = ElaValue.FromObject(value);
				arguments.Add(name, val);
				return true;
			}
			else
				return false;
		}


		public IEnumerable<ForeignModule> EnumerateForeignModules()
		{
			return foreignModules;
		}


		public IEnumerable<String> EnumerateModules()
		{
			return moduleMap.Keys;
		}


		public IEnumerable<String> EnumerateArguments()
		{
			return arguments.Keys;
		}


		internal bool TryGetArgument(string name, out ElaValue arg)
		{
			return arguments.TryGetValue(name, out arg);
		}


		internal void RefreshRootModule(CodeFrame frame)
		{
			if (frame != null)
				modules[0] = frame;
		}


		internal void RefreshModule(int handle, CodeFrame frame)
		{
			modules[handle] = frame;
		}
		#endregion


		#region Properties
		public int ModuleCount
		{
			get { return modules.Count; }
		}


		public int ArgumentCount
		{
			get { return arguments.Count; }
		}
		#endregion
	}
}