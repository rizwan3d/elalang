using System;
using Ela.Linking;

namespace Ela.StandardLibrary.Debug
{
	public sealed class Module : ForeignModule
	{
		public override void Initialize()
		{
			Add("getList", new GetList());
			Add("getArray", new GetArray());
		}
	}
}
