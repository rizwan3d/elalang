using System;
using System.Collections.Generic;
using System.Text;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela
{
	public static class FormatHelper
	{
		#region Methods
        public static string FormatEnumerable(IEnumerable<ElaValue> seq, ExecutionContext ctx, ShowInfo info)
		{
			var sb = new StringBuilder();
			var c = 0;
			var maxLen = info.SequenceLength;

			foreach (var v in seq)
			{
				if (maxLen > 0 && c > maxLen)
				{
					sb.Append("...");
					break;
				}

				if (c++ > 0)
					sb.Append(',');

				if (v.Ref != null)
					sb.Append(v.Ref.Show(v, info, ctx));
			}

			return sb.ToString();
		}
		#endregion
	}
}
