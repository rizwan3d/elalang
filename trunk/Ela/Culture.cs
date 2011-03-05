using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Ela
{
	internal static class Culture
	{
		internal static readonly IFormatProvider NumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;
	}
}
