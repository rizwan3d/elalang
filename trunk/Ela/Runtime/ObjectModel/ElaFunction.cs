﻿using System;
using System.Text;
using Ela.Debug;
using Ela.CodeModel;
using System.Collections.Generic;

namespace Ela.Runtime.ObjectModel
{
	public class ElaFunction : ElaObject
	{
		#region Construction
		private ElaMachine vm;
		private static readonly ElaValue[] defaultParams = new ElaValue[] { new ElaValue(ElaUnit.Instance) };
		private static readonly ElaValue[] emptyParams = new ElaValue[0];
		private const string DEF_NAME = "<f>";
        private const string MODULE = "module";
        private const string HANDLE = "handle";
        private const string NAME = "name";
        private const string PARAMS = "parameters";
        private const string ISNATIVE = "isNative";
        private const string ISGLOBAL = "isGlobal";
        private const string ISPARTIAL = "isPartial";


		protected ElaFunction() : this(1)
		{

		}
		
		
		protected ElaFunction(int parCount) : base(ElaTypeCode.Function)
		{
			if (parCount == 0)
				parCount = 1;

			if (parCount > 1)
				Parameters = new ElaValue[parCount - 1];
			else
				Parameters = emptyParams;
		}

		
		internal ElaFunction(int handle, int module, int parCount, FastList<ElaValue[]> captures, ElaMachine vm) : base(ElaTypeCode.Function)
		{
			Handle = handle;
			ModuleHandle = module;
			Captures = captures;
			this.vm = vm;
			Parameters = parCount > 1 ? new ElaValue[parCount - 1] : emptyParams;
		}


		private ElaFunction(ElaValue[] pars) : base(ElaTypeCode.Function)
		{
			Parameters = pars;
		}
		#endregion


		#region Operations
		protected internal override ElaValue Call(ElaValue value, ExecutionContext ctx)
		{
			try
			{
				return vm != null ? vm.CallPartial(this, value) : Call(value);
			}
			catch (ElaCodeException ex)
			{
				ctx.Fail(ex.ErrorObject);
				return Default();
			}
		}


		internal ElaValue CallWithThrow(ElaValue value)
		{
			return vm.CallPartial(this, value);
		}


		protected internal override ElaValue Equal(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
			return new ElaValue(IsEqual(left.Ref, right.Ref));
		}


		protected internal override ElaValue NotEqual(ElaValue left, ElaValue right, ExecutionContext ctx)
		{
            return new ElaValue(!IsEqual(left.Ref, right.Ref));
		}


        private bool IsEqual(ElaObject leftObj, ElaObject rightObj)
        {
            var left = leftObj as ElaFunction;
            var right = leftObj as ElaFunction;

            if (left == null || right == null)
                return false;

            return Object.ReferenceEquals(left, right) ||
                left.Handle == right.Handle &&
                left.ModuleHandle == right.ModuleHandle &&
                left.AppliedParameters == right.AppliedParameters &&
                left.Flip == right.Flip &&
                EqHelper.ListEquals<ElaValue>(left.Parameters, right.Parameters) &&
                (Object.ReferenceEquals(left.LastParameter.Ref, right.LastParameter.Ref) ||
                    left.LastParameter.Equals(right.LastParameter));

        }


        protected internal override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
		{
			var sb = new StringBuilder();
			sb.Append("*");

			for (var i = 0; i < Parameters.Length + 1 - AppliedParameters; i++)
			{
				sb.Append("->");
				sb.Append("*");
			}

			return GetFunctionName() + ":" + sb.ToString();
		}
		#endregion


		#region Static Methods
		public static ElaFunction Create<T1>(ElaFun<T1> fun)
		{
			return new DelegateFunction<T1>(DEF_NAME, fun);
		}


		public static ElaFunction Create<T1,T2>(ElaFun<T1,T2> fun)
		{
			return new DelegateFunction<T1,T2>(DEF_NAME, fun);
		}


		public static ElaFunction Create<T1,T2,T3>(ElaFun<T1,T2,T3> fun)
		{
			return new DelegateFunction<T1,T2,T3>(DEF_NAME, fun);
		}


		public static ElaFunction Create<T1,T2,T3,T4>(ElaFun<T1,T2,T3,T4> fun)
		{
			return new DelegateFunction<T1,T2,T3,T4>(DEF_NAME, fun);
		}


		public static ElaFunction Create<T1,T2,T3,T4,T5>(ElaFun<T1,T2,T3,T4,T5> fun)
		{
			return new DelegateFunction<T1,T2,T3,T4,T5>(DEF_NAME, fun);
		}


		public static ElaFunction Create<T1>(string name, ElaFun<T1> fun)
		{
			return new DelegateFunction<T1>(name, fun);
		}


		public static ElaFunction Create<T1,T2>(string name, ElaFun<T1,T2> fun)
		{
			return new DelegateFunction<T1,T2>(name, fun);
		}


		public static ElaFunction Create<T1,T2,T3>(string name, ElaFun<T1,T2,T3> fun)
		{
			return new DelegateFunction<T1,T2,T3>(name, fun);
		}


		public static ElaFunction Create<T1,T2,T3,T4>(string name, ElaFun<T1,T2,T3,T4> fun)
		{
			return new DelegateFunction<T1,T2,T3,T4>(name, fun);
		}


		public static ElaFunction Create<T1,T2,T3,T4,T5>(string name, ElaFun<T1,T2,T3,T4,T5> fun)
		{
			return new DelegateFunction<T1,T2,T3,T4,T5>(name, fun);
		}
		#endregion


		#region Methods
        internal virtual ElaFunction Resolve(ElaValue arg, ExecutionContext ctx)
        {
            return this;
        }


        internal ElaFunction CloneFast()
		{
            if (Overloaded)
                return Clone();

			var pars = new ElaValue[Parameters.Length];

			if (AppliedParameters > 0) //This is faster than Array.Copy
				for (var i = 0; i < AppliedParameters; i++)
					pars[i] = Parameters[i];

			var ret = new ElaFunction(pars);
			ret.AppliedParameters = AppliedParameters;
			ret.Handle = Handle;
			ret.ModuleHandle = ModuleHandle;
			ret.vm = vm;
			ret.Captures = Captures;
			ret.Flip = Flip;
			return ret;
		}


		protected ElaFunction CloneFast(ElaFunction newInstance)
		{
			var pars = new ElaValue[Parameters.Length];

			if (AppliedParameters > 0) //This is faster than Array.Copy
				for (var i = 0; i < AppliedParameters; i++)
					pars[i] = Parameters[i];

			newInstance.Parameters = pars;
			newInstance.AppliedParameters = AppliedParameters;
			newInstance.Handle = Handle;
			newInstance.ModuleHandle = ModuleHandle;
			newInstance.vm = vm;
			newInstance.Captures = Captures;
			newInstance.Flip = Flip;
			return newInstance;
		}


		public virtual ElaFunction Clone()
		{
			var nf = (ElaFunction)MemberwiseClone();
			var pars = new ElaValue[Parameters.Length];

			if (AppliedParameters > 0) //This is faster than Array.Copy
				for (var i = 0; i < AppliedParameters; i++)
					pars[i] = Parameters[i];

			nf.Parameters = pars;
			return nf;
		}


		public virtual ElaValue Call(params ElaValue[] args)
		{
			if (args == null || args.Length == 0)
				args = defaultParams;

			return vm.Call(this, args);
		}


		public override ElaTypeInfo GetTypeInfo()
		{
			var info = base.GetTypeInfo();
            info.AddField(MODULE, new ElaModule(ModuleHandle, vm).GetTypeInfo());
            info.AddField(HANDLE, Handle);
            info.AddField(NAME, GetFunctionName());
            info.AddField(PARAMS, Parameters.Length + 1 - AppliedParameters);
            info.AddField(ISNATIVE, vm != null);
            info.AddField(ISGLOBAL, vm == null || Captures.Count == 1);
            info.AddField(ISPARTIAL, AppliedParameters > 0);
            return info;
		}


		protected virtual string GetFunctionName()
		{
			var funName = DEF_NAME;

			if (vm != null)
			{
				var mod = vm.Assembly.GetModule(ModuleHandle);
				var syms = mod.Symbols;

				if (syms != null)
				{
					var dr = new DebugReader(syms);
					var fs = dr.GetFunSymByHandle(Handle);

					if (fs != null && fs.Name != null)
						funName = fs.Name;
				}
				else
				{
					foreach (var sv in mod.GlobalScope.Locals)
					{
						var v = vm.GetVariableByHandle(ModuleHandle, sv.Value.Address);

						if (v.TypeId == ElaMachine.FUN && ((ElaFunction)v.Ref).Handle == Handle)
						{
							funName = sv.Key;
							break;
						}
					}
				}
			}

			if (funName != DEF_NAME && Format.IsSymbolic(funName))
				funName = "(" + funName + ")";

			return funName;
		}
		#endregion


		#region Properties
		internal ElaMachine Machine { get { return vm; } }

		internal int Handle { get; private set; }

		internal int ModuleHandle { get; private set; }

		internal FastList<ElaValue[]> Captures { get; private set; }
		
		internal int AppliedParameters { get; set; }

		internal ElaValue[] Parameters { get; set; }

		internal ElaValue LastParameter { get; set; }

        internal bool Overloaded { get; set; }

		private bool _flip;
		internal bool Flip 
		{
			get { return _flip; }
			set
			{
				if (Parameters.Length > 0)
					_flip = value;
			}
		}
		#endregion
	}


    internal sealed class ElaOverloadedFunction : ElaFunction
    {
        #region Construction
        private Dictionary<ParamInfo,ElaFunction> overloads;

        internal ElaOverloadedFunction(Dictionary<ParamInfo,ElaFunction> funs, FastList<ElaValue[]> captures, ElaMachine vm) :
            base(0, 0, 2, captures, vm)
        {
            overloads = funs;
            Overloaded = true;
        }
        #endregion


        #region Methods
        internal override ElaFunction Resolve(ElaValue arg, ExecutionContext ctx)
        {            
            var tag = /*arg.TypeId != ElaMachine.VAR ? null :*/ arg.GetTag(ctx);

            if (ctx.Failed)
                return null;

            var fun = default(ElaFunction);
            
            if (!overloads.TryGetValue(new ParamInfo(AppliedParameters, tag), out fun))
            {
                if (tag == null || !overloads.TryGetValue(new ParamInfo(AppliedParameters, null), out fun))
                {
                    //error
                    ctx.Fail("No overloaded for function");
                    return null;
                }
            }

            fun = fun.CloneFast();
            fun.AppliedParameters = AppliedParameters;

            for (var i = 0; i < AppliedParameters; i++)
                fun.Parameters[i] = Parameters[i];

            return fun;
        }


        public override ElaFunction Clone()
        {
            var newInst = new ElaOverloadedFunction(overloads, Captures, Machine);
            return CloneFast(newInst);
        }
        #endregion
    }
}
