using System;
using Ela.ModuleGenerator;
using Ela.Runtime.ObjectModel;
using Ela.Runtime;

namespace Ela.StandardLibrary.Modules
{
	public partial class MathModule
	{
		#region Construction
		public MathModule()
		{

		}
		#endregion


		#region Methods
		[Function("rnd")]
		internal int Randomize(int val)
		{
			var rnd = new Random();
			return rnd.Next(val);
		}


		[Function("acos")]
		internal double Acos(double val)
		{
			return Math.Acos(val);
		}


		[Function("asin")]
		internal double Asin(double val)
		{
			return Math.Asin(val);
		}


		[Function("atan")]
		internal double Atan(double val)
		{
			return Math.Atan(val);
		}


		[Function("atan2")]
		internal double Atan2(double x, double y)
		{
			return Math.Atan2(x, y);
		}


		[Function("ceil")]
		internal double Ceil(double val)
		{
			return Math.Ceiling(val);
		}


		[Function("cos")]
		internal double Cos(double val)
		{
			return Math.Cos(val);
		}


		[Function("cosh")]
		internal double Cosh(double val)
		{
			return Math.Cosh(val);
		}


		[Function("exp")]
		internal double Exp(double val)
		{
			return Math.Exp(val);
		}


		[Function("floor")]
		internal double Floor(double val)
		{
			return Math.Floor(val);
		}


		[Function("log")]
		internal double Log(double val)
		{
			return Math.Log(val);
		}


		[Function("log2")]
		internal double Log2(double val, double newBase)
		{
			return Math.Log(val, newBase);
		}


		[Function("log10")]
		internal double Log10(double val)
		{
			return Math.Log10(val);
		}


		[Function("round")]
		internal double Round(double val)
		{
			return Math.Round(val);
		}


		[Function("sin")]
		internal double Sin(double val)
		{
			return Math.Sin(val);
		}


		[Function("sinh")]
		internal double Sinh(double val)
		{
			return Math.Sinh(val);
		}


		[Function("sqrt")]
		internal double Sqrt(double val)
		{
			return Math.Sqrt(val);
		}


		[Function("tan")]
		internal double Tan(double val)
		{
			return Math.Tan(val);
		}


		[Function("tanh")]
		internal double Tanh(double val)
		{
			return Math.Tanh(val);
		}


		[Function("truc")]
		internal double Truncate(double val)
		{
			return Math.Truncate(val);
		}
		#endregion
	}
}
