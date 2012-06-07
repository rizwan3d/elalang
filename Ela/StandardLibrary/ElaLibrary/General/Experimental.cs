using System;
using Ela.Linking;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using System.Diagnostics;

namespace Ela.Library.General
{
    public sealed class Experimental : ForeignModule
    {
        #region Construction
        public Experimental()
        {

        }

        public class SubFun : ElaFunction
        {
            public SubFun()
                : base(2)
            {
                Spec = 2;
            }

            protected override ElaValue Call(ElaValue arg1, ElaValue arg2, ExecutionContext ctx)
            {
                return arg1.Subtract(arg1, arg2, ctx);
            }
        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<ElaValue,String,ElaObject>("newtype", NewType2);
            Add<ElaValue, Proxy>("newType", NewType);
            Add<ElaFunction, Proxy, Proxy>("add", Add);
            Add("sub", new SubFun());
            Add<ElaValue,ElaValue,ElaValue>("sum", (x,y)=>x.Add(x,y,null));
        }

        public Proxy NewType(ElaValue val)
        {
            return new Proxy(val);
        }

        public ElaObject NewType2(ElaValue val, string name)
        {
            return new CustomType(val, name);
        }

        public Proxy Add(ElaFunction fun, Proxy proxy)
        {
            proxy.SetAddFunction(fun);
            return proxy;
        }
        #endregion
    }


    public sealed class CustomType : ElaObject
    {
        private string name;

        public CustomType(ElaValue val, string name)
        {
            Value = val;
            this.name = name;
        }

        protected override string GetTypeName()
        {
            return name;
        }

        public ElaValue Value { get; private set; }
    }


    public sealed class Proxy : ElaProxy
    {
        private ElaFunction addFun;

        public Proxy(ElaValue val)
            : base(val)
        {

        }

        internal void SetAddFunction(ElaFunction addFun)
        {
            this.addFun = addFun;
        }

        protected override string Show(ElaValue @this, ShowInfo info, ExecutionContext ctx)
        {
            return "PROXY";
        }

        protected override ElaValue Add(ElaValue left, ElaValue right, ExecutionContext ctx)
        {
            if (addFun != null)
           //     return addFun.Call(Self(left), Self(right));
            {

                ctx.SetDeffered(addFun, 2);
                return Default();
            }
            else
                return base.Add(left, right, ctx);
        }
    }
}
