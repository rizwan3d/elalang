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
        private FastList<Boolean> quals;
        private FastList<ForeignModule> foreignModules;
		
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
            quals = new FastList<Boolean>();
		}
		#endregion


		#region Methods
        internal void RegisterForeignModule(ForeignModule module)
		{
			foreignModules.Add(module);
		}


		internal void AddModule(string name, CodeFrame module, bool qual)
		{
			var hdl = 0;

			if (!moduleMap.TryGetValue(name, out hdl))
			{
				moduleMap.Add(name, modules.Count);
				modules.Add(module);
				quals.Add(qual);
			}
			else
			{
				modules[hdl] = module;
				quals[hdl] = qual;
			}
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


        internal int TryGetModuleHandle(string name)
        {
            var val = 0;

            if (!moduleMap.TryGetValue(name, out val))
                return -1;

            return val;
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


		public IEnumerable<ForeignModule> EnumerateForeignModules()
		{
			return foreignModules;
		}


		public IEnumerable<String> EnumerateModules()
		{
			return moduleMap.Keys;
		}


		internal void RefreshRootModule(CodeFrame frame)
		{
			if (frame != null)
				modules[0] = frame;
		}


        internal bool RequireQuailified(int moduleHandle)
        {
            return quals[moduleHandle];
        }
		#endregion


		#region Properties
		public int ModuleCount
		{
			get { return modules.Count; }
		}
		#endregion
	}
}