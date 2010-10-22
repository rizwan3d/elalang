using System;
using System.Collections.Generic;
using System.Linq;
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
		private Dictionary<String,RuntimeValue> arguments;


		public CodeAssembly(CodeFrame frame) : this()
		{
			moduleMap.Add(MAIN_NAME, modules.Count);
			modules.Add(frame);			
		}


		internal CodeAssembly()
		{
			modules = new FastList<CodeFrame>();
			moduleMap = new Dictionary<String,Int32>();
			arguments = new Dictionary<String,RuntimeValue>();
		}
		#endregion


		#region Methods
		public bool AddModule(string name, ForeignModule module)
		{
			var frame = module.Compile();
			return AddModule(name, frame);
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
			return GetModule(moduleMap[name]);
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
			return moduleMap.First(kv => kv.Value == handle).Key;
		}


		public bool AddArgument(string name, object value)
		{
			if (!arguments.ContainsKey(name))
			{
				var val = RuntimeValue.FromObject(value);
				arguments.Add(name, val);
				return true;
			}
			else
				return false;
		}


		public IEnumerable<String> EnumerateModules()
		{
			return moduleMap.Keys.Cast<String>();
		}


		public IEnumerable<String> EnumerateArguments()
		{
			return arguments.Keys.Cast<String>();
		}


		internal bool TryGetArgument(string name, out RuntimeValue arg)
		{
			return arguments.TryGetValue(name, out arg);
		}


		internal void RefreshRootModule(CodeFrame frame)
		{
			if (frame != null)
				modules[0] = frame;
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