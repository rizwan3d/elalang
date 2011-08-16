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
        public static readonly ElaQueue Empty = new ElaQueue(ElaList.Empty, ElaList.Empty);
        private const string TAG = "Queue#";
        private ElaList forward;
        private ElaList backward;

        internal ElaQueue(ElaList forward, ElaList backward)
        {
            this.forward = forward;
            this.backward = backward;
        }


		public ElaQueue(IEnumerable<ElaValue> seq) : this(ElaList.FromEnumerable(seq), ElaList.Empty)
        {

        }
        #endregion


        #region Methods
        public override string GetTag()
        {
            return TAG;
        }


        public override string ToString()
        {
            return "queue" + ToList().ToString();
        }


        internal ElaValue Head()
        {
            return Peek();
        }


        internal ElaQueue Tail()
        {
            return Dequeue();
        }


        internal bool IsNil()
        {
            return this == ElaQueue.Empty;
        }


		internal ElaQueue Cons(ElaQueue instance, ElaValue value)
		{
			return new ElaQueue(new ElaList(forward, value), backward);
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

            if (f != ElaList.Empty)
                return new ElaQueue(f, backward);
            else if (backward == ElaList.Empty)
                return ElaQueue.Empty;
            else
                return new ElaQueue(backward.Reverse(), ElaList.Empty);
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


		internal ElaList GetForwardList()
		{
			return forward;
		}


		internal ElaList GetBackwardList()
		{
			return backward;
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
