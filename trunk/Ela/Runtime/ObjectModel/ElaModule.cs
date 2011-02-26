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
		private const string MODULE = "[module:{0}]";
		private ElaMachine vm;

		internal ElaModule(int handle, ElaMachine vm) : base(ElaTypeCode.Module, ElaTraits.Eq | ElaTraits.Show)
		{
			Handle = handle;
			this.vm = vm;
		}
		#endregion


		#region Traits
		protected internal override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.TypeCode == right.TypeCode &&
				((ElaModule)left.Ref).Handle == ((ElaModule)right.Ref).Handle);
		}


		protected internal override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(left.TypeCode != right.TypeCode ||
				((ElaModule)left.Ref).Handle != ((ElaModule)right.Ref).Handle);
		}


		protected internal override string Show(ExecutionContext ctx, ShowInfo info)
		{
			return String.Format(MODULE, vm != null ? vm.Assembly.GetModuleName(Handle) : String.Empty);
		}
		#endregion


		#region Methods
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
					var val = vm.GetVariableByHandle(Handle, sv.Address);

					if (val.Ref != null)
						variables.Add(new ElaVariableInfo(Handle, sv.Address, v, val,
							(sv.Flags & ElaVariableFlags.Immutable) == ElaVariableFlags.Immutable,
							(sv.Flags & ElaVariableFlags.Private) == ElaVariableFlags.Private));
				}

				var refs = new List<ElaReferenceInfo>();

				foreach (var kv in frame.References)
					refs.Add(new ElaReferenceInfo(kv.Value.ModuleName, kv.Value.DllName, kv.Key, 
						String.Join(System.IO.Path.DirectorySeparatorChar.ToString(), kv.Value.Path)));

				return new ElaModuleInfo(this, vm, Handle, name,
					frame.File != null ? frame.File.FullName : null, !(frame is IntrinsicFrame), variables, refs);
			}
			else
				return new ElaModuleInfo(this, null, Handle, null, null, false, null, null);
		}
		#endregion


		#region Properties
		public int Handle { get; private set; }
		#endregion
	}
}
