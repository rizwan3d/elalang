using System;
using System.Collections.Generic;
using System.Linq;
using Ela.Runtime.ObjectModel;
using Ela.Linking;

namespace Ela.Runtime.Reflection
{
	public class ElaAssemblyInfo : ElaInfo, IEquatable<ElaAssemblyInfo>
	{
		#region Construction
		private const string MODULECOUNT = "moduleCount";
		private const string ARGCOUNT = "argumentCount";
		private const string ISREGISTERED = "isModuleRegistered";
		private const string GETROOTMODULE = "getRootModule";
		private const string GETMODULES = "getModules";
		private const string GETMODULE = "getModule";
		private const string GETARGUMENTS = "getArguments";
		private const string GETARGVALUE = "getArgValue";
		private ElaMachine vm;

		internal ElaAssemblyInfo(ElaMachine vm) : base(8)
		{
			this.vm = vm;
			Assembly = vm.Assembly;


			AddField(0, MODULECOUNT, new ElaValue(Assembly.ModuleCount));
			AddField(1, ARGCOUNT, new ElaValue(Assembly.ArgumentCount));
			AddField(2, ISREGISTERED, new ElaValue(new IsRegisteredFunction(Assembly)));
			AddField(3, GETROOTMODULE, new ElaValue(new GetRootModuleFunction(this)));
			AddField(4, GETMODULES, new ElaValue(new GetModuleFunction(this)));
			AddField(5, GETMODULE, new ElaValue(new GetModuleFunction(this)));
			AddField(6, GETARGUMENTS, new ElaValue(new GetArgumentFunction(Assembly)));
			AddField(7, GETARGVALUE, new ElaValue(new GetArgumentFunction(Assembly)));
		}
		#endregion


		#region Functions
		private sealed class IsRegisteredFunction : ElaFunction
		{
			private CodeAssembly asm;

			internal IsRegisteredFunction(CodeAssembly asm) : base(1)
			{
				this.asm = asm;
			}

			public override ElaValue Call(params ElaValue[] args)
			{
				return new ElaValue(asm.IsModuleRegistered(args[0].ToString()));
			}
		}


		private sealed class GetRootModuleFunction : ElaFunction
		{
			private ElaAssemblyInfo obj;

			internal GetRootModuleFunction(ElaAssemblyInfo obj) : base(0)
			{
				this.obj = obj;
			}

			public override ElaValue Call(params ElaValue[] args)
			{
				return new ElaValue(new ElaModule(0, obj.vm).GetTypeInfo());
			}
		}


		private sealed class GetModulesFunction : ElaFunction
		{
			private ElaAssemblyInfo obj;

			internal GetModulesFunction(ElaAssemblyInfo obj) : base(0)
			{
				this.obj = obj;
			}

			public override ElaValue Call(params ElaValue[] args)
			{
				return new ElaValue(new ElaArray(obj.Assembly.EnumerateModules().Select(
					m => new ElaModule(obj.Assembly.GetModuleHandle(m), obj.vm)).ToArray()));
			}
		}


		private sealed class GetModuleFunction : ElaFunction
		{
			private ElaAssemblyInfo obj;

			internal GetModuleFunction(ElaAssemblyInfo obj) : base(1)
			{
				this.obj = obj;
			}

			public override ElaValue Call(params ElaValue[] args)
			{
				return new ElaValue(new ElaModule(obj.Assembly.GetModuleHandle(args[0].ToString()), obj.vm));
			}
		}


		private sealed class GetArgumentsFunction : ElaFunction
		{
			private CodeAssembly asm;

			internal GetArgumentsFunction(CodeAssembly asm) : base(0)
			{
				this.asm = asm;
			}

			public override ElaValue Call(params ElaValue[] args)
			{
				return new ElaValue(new ElaArray(asm.EnumerateArguments().ToArray()));
			}
		}


		private sealed class GetArgumentFunction : ElaFunction
		{
			private CodeAssembly asm;

			internal GetArgumentFunction(CodeAssembly asm) : base(1)
			{
				this.asm = asm;
			}

			public override ElaValue Call(params ElaValue[] args)
			{
				var val = default(ElaValue);
				return asm.TryGetArgument(args[0].ToString(), out val) ? val :
					new ElaValue(ElaUnit.Instance);
			}
		}
		#endregion


		#region Methods
		public static bool Equals(ElaAssemblyInfo lho, ElaAssemblyInfo rho)
		{
			return Object.ReferenceEquals(lho, rho) ||
				!Object.ReferenceEquals(lho, null) && !Object.ReferenceEquals(rho, null) &&
				Object.ReferenceEquals(lho.Assembly, rho.Assembly);
		}


		public bool Equals(ElaAssemblyInfo other)
		{
			return Equals(this, other);
		}


		public override bool Equals(object obj)
		{
			return Equals(obj as ElaAssemblyInfo);
		}


		public override int GetHashCode()
		{
			return Assembly.GetHashCode();
		}
		#endregion


		#region Properties
		public CodeAssembly Assembly { get; private set; }
		#endregion


		#region Operators
		public static bool operator ==(ElaAssemblyInfo lho, ElaAssemblyInfo rho)
		{
			return Equals(lho, rho);
		}


		public static bool operator !=(ElaAssemblyInfo lho, ElaAssemblyInfo rho)
		{
			return !Equals(lho, rho);
		}
		#endregion
	}
}
