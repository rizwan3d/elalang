using System;
using Ela.Linking;

namespace Ela.StandardLibrary
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
			Add<Int64>("startClock", StartClock);
			Add<Int64,String>("stopClock", StopClock);
		}


		public long StartClock()
		{
			return DateTime.Now.Ticks;
		}


		public string StopClock(long val)
		{
			return ((Int32)(DateTime.Now - new DateTime(val)).TotalMilliseconds).ToString();
		}
		#endregion
	}
}
