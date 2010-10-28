using System;
using System.IO;
using Ela.Compilation;

namespace Ela.Linking
{
	public sealed class ObjectFileWriter : ObjectFile
	{
		#region Construction
		public ObjectFileWriter(FileInfo file) : base(file)
		{

		}
		#endregion


		#region Methods
		public void Write(CodeFrame frame)
		{
			using (var bw = new BinaryWriter(File.OpenWrite()))
				Write(frame, bw);
		}


		private void Write(CodeFrame frame, BinaryWriter bw)
		{
			bw.Write(Version);
			bw.Write(frame.References.Count);

			foreach (var kv in frame.References)
			{
				bw.Write(kv.Key);
				bw.Write(kv.Value.ModuleName);
				bw.Write(kv.Value.DllName);
				bw.Write(kv.Value.Folder);
			}

			bw.Write(frame.DeclaredPervasives.Count);

			foreach (var kv in frame.DeclaredPervasives)
			{
				bw.Write(kv.Key);
				bw.Write(kv.Value);
			}

			bw.Write(frame.GlobalScope.Locals.Count);

			foreach (var kv in frame.GlobalScope.Locals)
			{
				bw.Write(kv.Key);
				bw.Write((Int32)kv.Value.Flags);
				bw.Write(kv.Value.Address);
			}

			bw.Write(frame.Layouts.Count);

			for (var i = 0; i < frame.Layouts.Count; i++)
			{
				var l = frame.Layouts[i];
				bw.Write(l.Size);
				bw.Write(l.Address);
			}

			bw.Write(frame.Strings.Count);

			for (var i = 0; i < frame.Strings.Count; i++)
				bw.Write(frame.Strings[i]);

			var ops = frame.Ops;
			bw.Write(ops.Count);

			for (var i = 0; i < ops.Count; i++)
			{
				var op = ops[i];
				bw.Write((Byte)op);

				if (OpHelper.OpSize[(Int32)op] > 1)
					bw.Write(frame.OpData[i]);
			}
		}
		#endregion
	}
}
