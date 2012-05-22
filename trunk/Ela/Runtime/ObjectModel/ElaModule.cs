using System;
using System.Collections.Generic;
using Ela.CodeModel;
using Ela.Linking;
using Ela.Compilation;

namespace Ela.Runtime.ObjectModel
{
	public sealed class ElaModule : ElaObject
	{
		#region Construction
        internal static readonly ElaTypeInfo TypeInfo = new ElaTypeInfo(TypeCodeFormat.GetShortForm(ElaTypeCode.Module), (Int32)ElaTypeCode.Module, true, typeof(ElaModule));
        
        private const string GLOBALS = "globals";
        private const string REFERENCES = "references";
        private const string ASSEMBLY = "assembly";
        private const string HANDLE = "handle";
        private const string NAME = "name";
        private const string CODEBASE = "codeBase";
        private const string ISNATIVE = "isNative";
        private const string ISMAINMODULE = "isMainModule";
        private const string ADDRESS = "address";
        private const string VARNAME = "name";
        private const string ISPRIVATE = "isPrivate";
        private const string VALUE = "value";
        private const string MODULENAME = "moduleName";
        private const string DLLNAME = "dllName";
        private const string ALIAS = "alias";
        private const string PATH = "path";
        private const string MODULECOUNT = "moduleCount";
        private const string MODULE = "[module:{0}]";
		private ElaMachine vm;

		internal ElaModule(int handle, ElaMachine vm) : base(ElaTypeCode.Module)
		{
			Handle = handle;
			this.vm = vm;
		}
		#endregion


		#region Operations
		protected internal override bool Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.TypeCode == right.TypeCode &&
				((ElaModule)left.Ref).Handle == ((ElaModule)right.Ref).Handle;
		}


		protected internal override bool NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return left.TypeCode != right.TypeCode ||
				((ElaModule)left.Ref).Handle != ((ElaModule)right.Ref).Handle;
		}


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			return String.Format(MODULE, vm != null ? vm.Assembly.GetModuleName(Handle) : String.Empty);
		}


        protected internal override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
		{
			if (vm != null)
			{
                if (index.TypeId != ElaMachine.STR)
                {
                    ctx.InvalidIndexType(index);
                    return Default();
                }

                var field = index.DirectGetString();
				var frame = vm.Assembly.GetModule(Handle);
				ScopeVar sc;

				if (!frame.GlobalScope.Locals.TryGetValue(field, out sc))
				{
					ctx.IndexOutOfRange(index, new ElaValue(this));
					return Default();
				}

				if ((sc.Flags & ElaVariableFlags.Private) == ElaVariableFlags.Private)
				{
					ctx.Fail(new ElaError(ElaRuntimeError.PrivateVariable, field));
					return Default();
				}

				return vm.modules[Handle][sc.Address];
			}

            ctx.Fail(ElaRuntimeError.Unknown, "VM is non present");
			return Default();
		}


		protected internal override bool Has(string field, ExecutionContext ctx)
		{
			if (vm != null)
			{
				var frame = vm.Assembly.GetModule(Handle);
				ScopeVar sc;

				if (!frame.GlobalScope.Locals.TryGetValue(field, out sc) || 
					(sc.Flags & ElaVariableFlags.Private) == ElaVariableFlags.Private)
					return false;

				return true;
			}

			return false;
		}
		#endregion


		#region Methods
        public override ElaPatterns GetSupportedPatterns()
        {
            return ElaPatterns.Record;
        }


		public override ElaTypeInfo GetTypeInfo()
		{
            var frame = vm != null ? vm.Assembly.GetModule(Handle) : null;
            var name = vm != null ? vm.Assembly.GetModuleName(Handle) : null;

            return CreateTypeInfo(
                new ElaRecordField(GLOBALS, GetVariables(frame)),
                new ElaRecordField(REFERENCES, GetReferences(frame)),
                new ElaRecordField(ASSEMBLY, GetAssemblyInfo()),
                new ElaRecordField(HANDLE, Handle),
                new ElaRecordField(NAME, name),
                new ElaRecordField(CODEBASE, frame != null && frame.File != null ? frame.File.FullName : null),
                new ElaRecordField(ISNATIVE, frame != null ? new ElaValue(!(frame is IntrinsicFrame)) : new ElaValue(ElaUnit.Instance)),
                new ElaRecordField(ISMAINMODULE, Handle == 0)
            );
		}


        private IEnumerable<ElaRecord> GetVariables(CodeFrame frame)
        {
            if (frame != null)
            {
                foreach (var v in frame.GlobalScope.EnumerateNames())
                {
					var sv = frame.GlobalScope.GetVariable(v);

					if ((sv.Flags & ElaVariableFlags.Private) != ElaVariableFlags.Private)
					{
						var val = vm.GetVariableByHandle(Handle, sv.Address);

						if (val.Ref != null)
							yield return new ElaRecord(
								new ElaRecordField(ADDRESS, sv.Address, false),
								new ElaRecordField(VARNAME, v, false),
								new ElaRecordField(VALUE, val, false),
								new ElaRecordField(ISPRIVATE, (sv.Flags & ElaVariableFlags.Private) == ElaVariableFlags.Private, false));
					}
                }
            }
        }


        private IEnumerable<ElaRecord> GetReferences(CodeFrame frame)
        {
            if (frame != null)
            {
                foreach (var kv in frame.References)
                    yield return new ElaRecord(
                        new ElaRecordField(MODULENAME, kv.Value.ModuleName, false),
                        new ElaRecordField(DLLNAME, kv.Value.DllName, false),
                        new ElaRecordField(ALIAS, kv.Key, false),
                        new ElaRecordField(PATH, String.Join(System.IO.Path.DirectorySeparatorChar.ToString(), kv.Value.Path), false));
            }
        }


        private ElaObject GetAssemblyInfo()
        {
            if (vm != null)
                return new ElaRecord(
                    new ElaRecordField(MODULECOUNT, vm.Assembly.ModuleCount, false));
            else
                return ElaUnit.Instance;
        }
		#endregion


		#region Properties
		public int Handle { get; private set; }
		#endregion
	}
}
