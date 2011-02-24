using System;
using System.Collections;
using System.Collections.Generic;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class StackModule : ForeignModule
    {
        #region Construction
        private static readonly ElaStack empty = new ElaStack(ElaList.GetNil());

        public StackModule()
        {

        }
        #endregion


        #region Nested Classes
        public sealed class ElaStack : ElaObject, IEnumerable<ElaValue>
        {
            #region Construction
            internal ElaStack(ElaList list) : base(ElaTraits.Eq|ElaTraits.Show|ElaTraits.Get|ElaTraits.Seq|ElaTraits.Convert|ElaTraits.Len|ElaTraits.Fold|ElaTraits.Gen)
            {
                List = list;
            }
            #endregion


            #region Methods
            public IEnumerator<ElaValue> GetEnumerator()
            {
                return List.GetEnumerator();
            }
            

            IEnumerator IEnumerable.GetEnumerator()
            {
                return List.GetEnumerator();
            }


            protected override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
            {
                return new ElaValue(left.ReferenceEquals(right));
            }


            protected override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
            {
                return new ElaValue(!left.ReferenceEquals(right));
            }


            protected override string Show(ExecutionContext ctx, ShowInfo info)
            {
                return "stack" + new ElaValue(List).Show(ctx, info);
            }


            protected override ElaValue Convert(ObjectType type, ExecutionContext ctx)
            {
                if (type == ObjectType.List)
                    return new ElaValue(List);

                ctx.ConversionFailed(new ElaValue(this), type);
                return Default();
            }


            protected override ElaValue GetValue(ElaValue index, ExecutionContext ctx)
            {
                return new ElaValue(List).GetValue(index, ctx);
            }


            protected override ElaValue GetLength(ExecutionContext ctx)
            {
                return new ElaValue(List.Length);
            }


            protected override ElaValue Head(ExecutionContext ctx)
            {
                return List.Value;
            }


            protected override ElaValue Tail(ExecutionContext ctx)
            {
                return new ElaValue(new ElaStack(List.Next));
            }


            protected override bool IsNil(ExecutionContext ctx)
            {
                return new ElaValue(List).IsNil(ctx);
            }


            protected override ElaValue Generate(ElaValue value, ExecutionContext ctx)
            {
                return new ElaValue(new ElaStack(new ElaList(List, value)));
            }


            protected override ElaValue GenerateFinalize(ExecutionContext ctx)
            {
                return new ElaValue(new ElaStack(List.Reverse()));
            }
            #endregion


            #region Properties
            internal ElaList List { get; private set; }
            #endregion
        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add("empty", empty);
            Add<ElaList,ElaStack>("stack", CreateStack);
            Add<ElaValue,ElaStack,ElaStack>("push", Push);
            Add<ElaStack,ElaStack>("pop", Pop);
            Add<ElaStack,ElaValue>("get", GetValue);
        }


        public ElaStack CreateStack(ElaList list)
        {
            return new ElaStack(list);
        }


        public ElaStack Push(ElaValue item, ElaStack stack)
        {
            var list = new ElaList(stack.List, item);
            return new ElaStack(list);
        }


        public ElaStack Pop(ElaStack stack)
        {
            if (stack.List.Next == null)
                throw new ElaCallException("Stack is empty.");

            return new ElaStack(stack.List.Next);
        }


        public ElaValue GetValue(ElaStack stack)
        {
            if (stack.List.Next == null)
                throw new ElaCallException("Stack is empty.");

            return stack.List.Value;
        }
        #endregion
    }
}