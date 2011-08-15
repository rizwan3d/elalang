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
        private const string TAG = "Queue#";
        private ElaList forward;
        private ElaList backward;

        internal ElaQueue(ElaList forward, ElaList backward, TypeId typeId) : base(typeId)
        {
            this.forward = forward;
            this.backward = backward;
        }


		public ElaQueue(IEnumerable<ElaValue> seq, TypeId typeId) : this(ElaList.FromEnumerable(seq), ElaList.Empty, typeId)
        {

        }


        public ElaQueue(IEnumerable<ElaValue> seq, ElaMachine vm) : this(seq, vm.GetTypeId(TAG))
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
            return forward == ElaList.Empty && backward == ElaList.Empty;
        }


		internal ElaQueue Cons(ElaQueue instance, ElaValue value)
		{
			return new ElaQueue(new ElaList(forward, value), backward, new TypeId(base.TypeId));
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
            return new ElaQueue(forward, new ElaList(backward, value), new TypeId(base.TypeId));
        }


        public ElaQueue Dequeue()
        {
            var f = forward.Next;

            if (f != ElaList.Empty)
				return new ElaQueue(f, backward, new TypeId(base.TypeId));
            else if (backward == ElaList.Empty)
                return new ElaQueue(ElaList.Empty, ElaList.Empty, new TypeId(base.TypeId));
            else
                return new ElaQueue(backward.Reverse(), ElaList.Empty, new TypeId(base.TypeId));
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
