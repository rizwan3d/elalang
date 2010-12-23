using System;
using Ela.ModuleGenerator;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;
using Ela.Compilation;
using Ela.Debug;

namespace Ela.StandardLibrary.Modules
{
	public partial class DebugModule
	{
		#region Construction
		public DebugModule()
		{

		}
		#endregion


		#region Methods
		[Function("print")]
		internal void Print(ElaValue value)
		{
			Console.WriteLine(value);
		}


		[Function("fake3")]
		internal void Fake3(ElaValue p1, ElaValue p2, ElaValue p3)
		{
			
		}


		[Function("callMe")]
		internal ElaValue CallMe(ElaValue p1, ElaValue p2, ElaValue p3, ElaFunction fun)
		{
			return fun.Call(p1, p2, p3);
		}



		[Function("getArray")]
		internal ElaArray GetArray(int size)
		{
			var arr = new ElaArray(size);

			for (var i = 0; i < size; i++)
				arr.Add(new ElaValue(i));

			return arr;
		}


		[Function("getList")]
		internal ElaList GetList(int size)
		{
			var list = ElaList.GetNil();

			for (var i = size; i > -1; --i)
				list = new ElaList(list, new ElaValue(i));

			return list;
		}


		[Function("startClock")]
		internal long StartClock()
		{
			return DateTime.Now.Ticks;
		}


		[Function("stopClock")]
		internal string StopClock(long val)
		{
			return (DateTime.Now - new DateTime(val)).ToString();
		}
		#endregion
	}
}
