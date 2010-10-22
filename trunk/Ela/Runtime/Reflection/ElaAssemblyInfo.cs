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

		internal ElaAssemblyInfo(ElaMachine vm)
		{
			this.vm = vm;
			Assembly = vm.Assembly;
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

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return new RuntimeValue(asm.IsModuleRegistered(args[0].ToString()));
			}
		}


		private sealed class GetRootModuleFunction : ElaFunction
		{
			private ElaAssemblyInfo obj;

			internal GetRootModuleFunction(ElaAssemblyInfo obj) : base(0)
			{
				this.obj = obj;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return new RuntimeValue(new ElaModule(0, obj.vm).GetTypeInfo());
			}
		}


		private sealed class GetModulesFunction : ElaFunction
		{
			private ElaAssemblyInfo obj;

			internal GetModulesFunction(ElaAssemblyInfo obj) : base(0)
			{
				this.obj = obj;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return new RuntimeValue(new ElaArray(obj.Assembly.EnumerateModules().Select(
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

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return new RuntimeValue(new ElaModule(obj.Assembly.GetModuleHandle(args[0].ToString()), obj.vm));
			}
		}


		private sealed class GetArgumentsFunction : ElaFunction
		{
			private CodeAssembly asm;

			internal GetArgumentsFunction(CodeAssembly asm) : base(0)
			{
				this.asm = asm;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				return new RuntimeValue(new ElaArray(asm.EnumerateArguments().ToArray()));
			}
		}


		private sealed class GetArgumentFunction : ElaFunction
		{
			private CodeAssembly asm;

			internal GetArgumentFunction(CodeAssembly asm) : base(1)
			{
				this.asm = asm;
			}

			public override RuntimeValue Call(params RuntimeValue[] args)
			{
				var val = default(RuntimeValue);
				return asm.TryGetArgument(args[0].ToString(), out val) ? val :
					new RuntimeValue(ElaObject.Unit);
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


		internal protected override RuntimeValue GetAttribute(string name)
		{
			switch (name)
			{
				case MODULECOUNT: return new RuntimeValue(Assembly.ModuleCount);
				case ARGCOUNT: return new RuntimeValue(Assembly.ArgumentCount);
				case ISREGISTERED: return new RuntimeValue(new IsRegisteredFunction(Assembly));
				case GETROOTMODULE: return new RuntimeValue(new GetRootModuleFunction(this));
				case GETMODULES: return new RuntimeValue(new GetModuleFunction(this));
				case GETMODULE: return new RuntimeValue(new GetModuleFunction(this));
				case GETARGUMENTS: return new RuntimeValue(new GetArgumentFunction(Assembly));
				case GETARGVALUE: return new RuntimeValue(new GetArgumentFunction(Assembly));

				default: return base.GetAttribute(name);
			}
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
