﻿using System;
using Ela.Linking;
using Ela.Runtime.ObjectModel;
using System.Threading;
using Ela.Runtime;
using System.Collections.Generic;

namespace Ela.Library.General
{
	public sealed class AsyncModule : ForeignModule
	{
		#region Construction
		public AsyncModule()
		{
			Threads = new List<Thread>();
		}
		#endregion


		#region Methods
		public override void Close()
		{
			foreach (var t in Threads)
			{
				if (t.IsAlive)
				{
					try
					{
						t.Abort();
					}
					catch { }
				}
			}
		}


		public override void Initialize()
		{
			Add<ElaFunction,ElaAsync>("async", RunAsync);
			Add<ElaAsync,ElaValue>("get", GetValue);
			Add<ElaAsync,Boolean>("hasValue", HasValue);
			Add<Int32,ElaAsync,ElaUnit>("wait", Wait);
			Add<ElaObject,ElaFunction,ElaUnit>("sync", Sync);
		}


		public ElaAsync RunAsync(ElaFunction fun)
		{
			var ret = new ElaAsync(this, fun);
			ret.Run();
			return ret;
		}


		public ElaValue GetValue(ElaAsync obj)
		{
			if (HasValue(obj))
				return obj.Return;
			else
			{
				Wait(5000, obj);

				if (!HasValue(obj))
					throw new ElaRuntimeException("AsyncNoValue", "Unable to obtain a value of an async computation.");

				return obj.Return;
			}
		}


		public bool HasValue(ElaAsync obj)
		{
			lock (obj.SyncRoot)
				return obj.Return.As<ElaObject>() != null;
		}


		public ElaUnit Wait(int timeout, ElaAsync obj)
		{
			var th = obj.Thread;

			if (th != null)
			{
				if (th.Join(timeout))
				{
					Threads.Remove(th);
					obj.Thread = null;
				}
			}

			return ElaUnit.Instance;
		}


		public ElaUnit Sync(ElaObject obj, ElaFunction fun)
		{
			lock (obj)
				fun.Call();

			return ElaUnit.Instance;
		}
		#endregion


		#region Properties
		public List<Thread> Threads { get; private set; }
		#endregion
	}
}
