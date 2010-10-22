using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Linking;
using Ela.Runtime.Reflection;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaModule : ElaObject
	{
		#region Construction
		private ElaMachine vm;

		internal ElaModule(int handle, ElaMachine vm) : base(ObjectType.Module)
		{
			Handle = handle;
			this.vm = vm;
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return Handle.ToString();
		}


		public override ElaTypeInfo GetTypeInfo()
		{
			if (vm != null)
			{
				var frame = vm.Assembly.GetModule(Handle);
				var name = vm.Assembly.GetModuleName(Handle);

				var variables = new List<ElaVariableInfo>();

				foreach (var v in frame.GlobalScope.EnumerateNames())
				{
					var sv = frame.GlobalScope.GetVariable(v);
					variables.Add(new ElaVariableInfo(Handle, sv.Address, v, vm.GetVariableByHandle(Handle, sv.Address),
						(sv.Flags & ElaVariableFlags.Immutable) == ElaVariableFlags.Immutable,
						(sv.Flags & ElaVariableFlags.Private) == ElaVariableFlags.Private,
						(sv.Flags & ElaVariableFlags.Constructor) == ElaVariableFlags.Constructor));
				}

				var variants = new List<ElaVariantInfo>();

				foreach (var kv in frame.Variants)
					variants.Add(new ElaVariantInfo(kv.Key, kv.Value));

				var refs = new List<ElaReferenceInfo>();

				foreach (var kv in frame.References)
					refs.Add(new ElaReferenceInfo(kv.Value.ModuleName, kv.Value.DllName, kv.Key, kv.Value.Folder));

				return new ElaModuleInfo(vm, Handle, name,
					frame.File != null ? frame.File.FullName : null, !(frame is IntrinsicFrame), variables, variants, refs);
			}
			else
				return new ElaModuleInfo(null, Handle, null, null, false, null, null, null);
		}
		#endregion


		#region Properties
		public int Handle { get; private set; }
		#endregion
	}
}
