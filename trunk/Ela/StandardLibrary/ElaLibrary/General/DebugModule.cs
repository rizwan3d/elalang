﻿using System;
using Ela.Linking;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using System.Diagnostics;
using System.Threading;

namespace Ela.Library.General
{
	public sealed class DebugModule : ForeignModule
	{
		#region Construction
		public DebugModule()
		{

		}
		#endregion
		
		
		#region Methods
		public override void Initialize()
		{
			Add<ElaObject>("startClock", StartClock);
            Add<Wrapper<Stopwatch>,String>("stopClock", StopClock);
			Add<Int32,ElaUnit>("sleep", Sleep);
		}


		public ElaObject StartClock()
		{
            var t = new Stopwatch();
            t.Start();
            return new Wrapper<Stopwatch>(t);
		}


		public string StopClock(Wrapper<Stopwatch> val)
		{
            val.Value.Stop();
            return val.Value.Elapsed.ToString();
		}


		public ElaUnit Sleep(int ms)
		{
			Thread.Sleep(ms);
			return ElaUnit.Instance;
		}
		#endregion
	}
}