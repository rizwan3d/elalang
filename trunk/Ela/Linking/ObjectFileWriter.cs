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
            
            var v = new Version(Const.Version);
            bw.Write(v.Major);
            bw.Write(v.Minor);
            bw.Write(v.Build);
            bw.Write(v.Revision);

            var d = DateTime.Now.ToUniversalTime().Ticks;
            bw.Write(d);

            bw.Write(frame.References.Count);

			foreach (var kv in frame.References)
			{
				bw.Write(kv.Key);
				bw.Write(kv.Value.ModuleName);
				bw.Write(kv.Value.DllName ?? String.Empty);
                bw.Write(kv.Value.RequireQuailified);
                bw.Write(kv.Value.Path.Length);
                
				foreach (var p in kv.Value.Path)
					bw.Write(p);

                bw.Write(kv.Value.LogicalHandle);
			}

			bw.Write(frame.GlobalScope.Locals.Count);

			foreach (var kv in frame.GlobalScope.Locals)
			{
				bw.Write(kv.Key);
				bw.Write((Int32)kv.Value.Flags);
				bw.Write(kv.Value.Address);
				bw.Write(kv.Value.Data);
			}

            bw.Write(frame.LateBounds.Count);

            foreach (var u in frame.LateBounds)
            {
                bw.Write(u.Name);
                bw.Write(u.Address);
                bw.Write(u.Data);
                bw.Write(u.Line);
                bw.Write(u.Column);
            }

			bw.Write(frame.Layouts.Count);

			for (var i = 0; i < frame.Layouts.Count; i++)
			{
				var l = frame.Layouts[i];
				bw.Write(l.Size);
				bw.Write(l.StackSize);
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

				if (OpSizeHelper.OpSize[(Int32)op] > 1)
					bw.Write(frame.OpData[i]);
			}
		}
		#endregion
	}
}
