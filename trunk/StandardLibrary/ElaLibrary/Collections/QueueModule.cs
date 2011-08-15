using System;
using System.Collections.Generic;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.Collections
{
    public sealed class QueueModule : ForeignModule
    {
        #region Construction
		private TypeId queueTypeId;

        public QueueModule()
        {

        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<IEnumerable<ElaValue>,ElaQueue>("queue", CreateQueue);
            Add<ElaQueue,ElaValue>("peek", Peek);
            Add<ElaQueue,ElaQueue>("dequeue", Dequeue);
            Add<ElaValue,ElaQueue,ElaQueue>("enqueue",Enqueue);
            Add<ElaQueue,ElaList>("toList", ToList);

			Add<ElaQueue,ElaList>("queueForwardList", q => q.GetForwardList());
			Add<ElaQueue,ElaList>("queueBackwardList", q => q.GetBackwardList());
			Add<ElaQueue,String>("toString", q => q.ToString());
			Add<ElaQueue,ElaValue>("queueHead", q => q.Head());
			Add<ElaQueue,ElaQueue>("queueTail", q => q.Tail());
			Add<ElaQueue,ElaQueue>("queueNil", _ => new ElaQueue(ElaList.Empty, ElaList.Empty, queueTypeId));
			Add<ElaQueue,Boolean>("queueIsNil", q => q.IsNil());
			Add<ElaQueue,ElaValue,ElaQueue>("queueCons", (q,v) => q.Cons(q, v));
			Add<ElaQueue,Int32>("queueLength", q => q.Length);
        }


		public override void RegisterTypes(TypeRegistrator registrator)
		{
			queueTypeId = registrator.ObtainTypeId("Queue#");
		}


        public ElaQueue CreateQueue(IEnumerable<ElaValue> seq)
        {
            return new ElaQueue(seq, queueTypeId);
        }


        public ElaValue Peek(ElaQueue queue)
        {
            return queue.Peek();
        }


        public ElaQueue Dequeue(ElaQueue queue)
        {
            return queue.Dequeue();
        }


        public ElaQueue Enqueue(ElaValue value, ElaQueue queue)
        {
            return queue.Enqueue(value);
        }


        public ElaList ToList(ElaQueue queue)
        {
            return queue.ToList();
        }
        #endregion
    }
}