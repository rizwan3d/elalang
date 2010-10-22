using System;
using Ela.Debug;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	internal sealed class ElaNativeFunction : ElaFunction
	{
		#region Construction
		private ElaMachine vm;


		internal ElaNativeFunction(int handle, int module, int parCount, FastList<RuntimeValue[]> captures, ElaMachine vm, 
			ObjectType type) : base(parCount)
		{
			Handle = handle;
			ModuleHandle = module;
			Captures = captures;
			this.vm = vm;
		}


		internal ElaNativeFunction(int handle, int module, int parCount, FastList<RuntimeValue[]> captures, ElaMachine vm) : 
			this(handle, module, parCount, captures, vm, ObjectType.Function)
		{
			
		}
		#endregion


		#region Methods
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			return vm != null ? vm.Call(this, args) : new RuntimeValue(Invalid);
		}


		public override ElaTypeInfo GetTypeInfo()
		{
			if (vm != null)
			{
				var dr = new DebugReader(vm.MainThread.Assembly.GetModule(ModuleHandle).Symbols);
				var sym = dr.GetFunSymByHandle(Handle);
				var name = sym != null ? sym.Name : null;
				return new ElaFunctionInfo((ElaModuleInfo)new ElaModule(ModuleHandle, vm).GetTypeInfo(), 
					Handle, name, ParameterCount, true, Memory != null, Captures != null && Captures.Count <= 1, 
					AppliedParameters != null);
			}
			else
				return base.GetTypeInfo();
		}
		#endregion


		#region Properties
		internal int Handle { get; private set; }

		internal int ModuleHandle { get; private set; }

		internal FastList<RuntimeValue[]> Captures { get; private set; }
		
		internal RuntimeValue[] Memory { get; set; }
		#endregion
	}
}
