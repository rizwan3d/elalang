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
		internal void Print(RuntimeValue value)
		{
			Console.WriteLine(ValueFormatter.FormatValue(value));
		}


		[Function("getArray")]
		internal ElaArray GetArray(int size)
		{
			var arr = new ElaArray(size);

			for (var i = 0; i < size; i++)
				arr[i] = new RuntimeValue(i);

			return arr;
		}


		[Function("getList")]
		internal ElaList GetList(int size)
		{
			var list = ElaList.Nil;

			for (var i = size; i > -1; --i)
				list = new ElaList(list, new RuntimeValue(i));

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
