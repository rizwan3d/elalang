using System;
using System.Collections;
using System.Collections.Generic;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class ElaQueue : ElaObject, IEnumerable<ElaValue>
    {
        #region Construction
        public static readonly ElaQueue Empty = new ElaQueue(ElaList.GetNil(), ElaList.GetNil());
        private const string TYPENAME = "queue";
        private ElaList forward;
        private ElaList backward;

        internal ElaQueue(ElaList forward, ElaList backward) : base(ElaTraits.Eq|ElaTraits.Show|ElaTraits.Len|ElaTraits.Convert|ElaTraits.Seq|ElaTraits.Cons)
        {
            this.forward = forward;
            this.backward = backward;
        }


        public ElaQueue(IEnumerable<ElaValue> seq) : this(ElaList.FromEnumerable(seq).Reverse(), ElaList.GetNil())
        {

        }
        #endregion


        #region Methods
        protected override string GetTypeName()
        {
            return TYPENAME;
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
            return "queue" + new ElaValue(ToList()).Show(ctx, info);
        }


        protected override ElaValue Head(ExecutionContext ctx)
        {
            return Peek();
        }


        protected override ElaValue Tail(ExecutionContext ctx)
        {
            return new ElaValue(Dequeue());
        }


        protected override bool IsNil(ExecutionContext ctx)
        {
            return this == Empty;
        }


        protected override ElaValue GetLength(ExecutionContext ctx)
        {
            return new ElaValue(Length);
        }


        protected override ElaValue Convert(ElaTypeCode type, ExecutionContext ctx)
        {
            if (type == ElaTypeCode.List)
                return new ElaValue(ToList());

            ctx.ConversionFailed(new ElaValue(this), type);
            return Default();
        }


		protected override ElaValue Cons(ElaObject instance, ElaValue value, ExecutionContext ctx)
		{
			var q = instance as ElaQueue;

			if (q == null)
			{
				ctx.InvalidType(GetTypeName(), new ElaValue(instance));
				return Default();
			}

			return new ElaValue(q.Enqueue(value));			
		}


		protected override ElaValue Nil(ExecutionContext ctx)
		{
			return new ElaValue(ElaQueue.Empty);
		}


        public ElaList ToList()
        {
            return forward.Concatenate(backward.Reverse());
        }


        public ElaValue Peek() 
        {
            return forward.Value;
        }


        public ElaQueue Enqueue(ElaValue value)
        {
            return new ElaQueue(forward, new ElaList(backward, value));
        }


        public ElaQueue Dequeue()
        {
            var f = forward.Next;

            if (!new ElaValue(f).IsNil(null))
                return new ElaQueue(f, backward);
            else if (new ElaValue(backward).IsNil(null))
                return ElaQueue.Empty;
            else
                return new ElaQueue(backward.Reverse(), ElaList.GetNil());
        }


        public IEnumerator<ElaValue> GetEnumerator()
        {
            foreach (var t in forward) 
                yield return t;
            
            foreach (var t in backward.Reverse()) 
                yield return t;
        }
        
        
        IEnumerator IEnumerable.GetEnumerator() 
        { 
            return ((IEnumerable<ElaValue>)this).GetEnumerator(); 
        }
        #endregion


        #region Properties
        public int Length
        {
            get { return forward.Length + backward.Length; }
        }
        #endregion
    }

}
