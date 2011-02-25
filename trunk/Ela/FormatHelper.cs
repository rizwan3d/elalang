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

			foreach (var v in seq)
			{
				if (info.SequenceLength > 0 && c > info.SequenceLength)
				{
					sb.Append("...");
					break;
				}

				if (c++ > 0)
					sb.Append(',');

				if (v.Ref != null)
					sb.Append(v.Ref.Show(v, ctx, info));
			}

			return sb.ToString();
		}
		#endregion
	}
}
