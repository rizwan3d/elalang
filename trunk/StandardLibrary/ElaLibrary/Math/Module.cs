using System;
using Ela.Linking;

namespace Ela.StandardLibrary.Math
{
	public sealed class Module : ForeignModule
	{
		public override void Initialize()
		{
			Add("acos", new Acos());
			Add("asin", new Asin());
			Add("atan", new Atan());
			Add("atan2", new Atan2());
			Add("tan", new Tan());
			Add("tanh", new Tanh());
			Add("cos", new Cos());
			Add("cosh", new Cosh());
			Add("sin", new Sin());
			Add("sinh", new Sinh());
			Add("sqrt", new Sqrt());
			Add("trunc", new Trunc());
			Add("round", new Round());
			Add("log", new Log());
			Add("log2", new Log2());
			Add("log10", new Log10());
			Add("floor", new Floor());
			Add("ceil", new Ceil());
			Add("exp", new Exp());
			Add("rnd", new Rnd());
		}
	}
}
