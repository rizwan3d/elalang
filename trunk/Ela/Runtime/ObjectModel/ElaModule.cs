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
        

		#region Methods
		public override string ToString()
		{
			return String.Format(MODULE, vm != null ? vm.Assembly.GetModuleName(Handle) : String.Empty);
		}


        internal override ElaValue Convert(ElaValue @this, ElaTypeCode typeCode, ExecutionContext ctx)
        {
            switch (typeCode)
            {
                case ElaTypeCode.Module: return @this;
                case ElaTypeCode.String: return new ElaValue(ToString());
                default: return base.Convert(@this, typeCode, ctx);
            }
        }


		public override string GetTag()
        {
            return "Module#";
        }


        public bool HasField(string field)
        {
            var frame = vm != null ? vm.Assembly.GetModule(Handle) : null;

            if (frame != null)
            {
                foreach (var v in frame.GlobalScope.EnumerateNames())
                {
                    if (v == field)
                        return true;
                }
            }

            return false;
        }


        public override ElaTypeInfo GetTypeInfo()
		{
            var frame = vm != null ? vm.Assembly.GetModule(Handle) : null;
            var name = vm != null ? vm.Assembly.GetModuleName(Handle) : null;

            var info = base.GetTypeInfo();
            info.AddField(GLOBALS, GetVariables(frame));
            info.AddField(REFERENCES, GetReferences(frame));
            info.AddField(ASSEMBLY, GetAssemblyInfo());
            info.AddField(HANDLE, Handle);
            info.AddField(NAME, name);
            info.AddField(CODEBASE, frame != null && frame.File != null ? frame.File.FullName : null);
            info.AddField(ISNATIVE, frame != null ? new ElaValue(!(frame is IntrinsicFrame)) : new ElaValue(ElaUnit.Instance));
            info.AddField(ISMAINMODULE, Handle == 0);
            return info;
		}


        private IEnumerable<ElaRecord> GetVariables(CodeFrame frame)
        {
            if (frame != null)
            {
                foreach (var kv in frame.GlobalScope.EnumerateVars())
                {
                    var v = kv.Key;
					var sv = kv.Value;

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

		internal ElaMachine Machine
		{
			get { return vm; }
		}
		#endregion
	}
}
