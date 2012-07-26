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
            Add<ElaUnit>("collect", Collect);
            Add<Int32,Int32,Int32,ElaList>("enumFromTo2", EnumFromTo);
        }

        public ElaUnit Collect()
        {
            GC.Collect();
            return ElaUnit.Instance;
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


        public ElaList EnumFromTo(int max, int fst, int snd)
        {
            var sw = new Stopwatch();
            sw.Start();

            var e = max;
            var xs = ElaList.Empty;
            snd = snd - fst;

            for (;;)
            {
                if (e < fst)
                    break;

                xs = new ElaList(xs, new ElaValue(e));
                e = e-snd;
            }

            var ret = xs;//.Reverse();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            return ret;
        }
		#endregion
	}
}