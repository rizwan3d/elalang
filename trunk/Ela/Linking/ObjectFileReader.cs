using System;
using System.IO;
using Ela.CodeModel;
using Ela.Compilation;

namespace Ela.Linking
{
	public sealed class ObjectFileReader : ObjectFile
	{
		#region Construction
		public ObjectFileReader(FileInfo file) : base(file)
		{

		}
		#endregion


		#region Methods
		public CodeFrame Read()
		{
			using (var bw = new BinaryReader(File.OpenRead()))
				return Read(bw);
		}
				

		private CodeFrame Read(BinaryReader bw)
		{
			var frame = new CodeFrame();
			frame.GlobalScope = new Scope(false, null);
			var v = bw.ReadInt32();

			if (v != Version)
				throw new ElaLinkerException(Strings.GetMessage("InvalidObjectFile", Version), null);

			var c = bw.ReadInt32();
			
			for (var i = 0; i < c; i++)
				frame.AddReference(bw.ReadString(),
					new ModuleReference(bw.ReadString(), bw.ReadString(), bw.ReadString(), 0, 0));
			
			c = bw.ReadInt32();

			for (var i = 0; i < c; i++)
				frame.GlobalScope.Locals.Add(bw.ReadString(),
					new ScopeVar((ElaVariableFlags)bw.ReadInt32(), bw.ReadInt32(), -1));

			c = bw.ReadInt32();

			for (var i = 0; i < c; i++)
				frame.DeclaredPervasives.Add(bw.ReadString(), bw.ReadInt32());
			
			c = bw.ReadInt32();

			for (var i = 0; i < c; i++)
			{
				var l = new MemoryLayout(bw.ReadInt32(), bw.ReadInt32());
				frame.Layouts.Add(l);
			}

			c = bw.ReadInt32();

			for (var i = 0; i < c; i++)
				frame.Strings.Add(bw.ReadString());

			c = bw.ReadInt32();

			for (var i = 0; i < c; i++)
			{
				var opCode = (Op)bw.ReadByte();
				frame.Ops.Add(opCode);
				frame.OpData.Add(OpHelper.OpSize[(Int32)opCode] > 1 ? bw.ReadInt32() : 0);
			}

			return frame;
		}
		#endregion
	}
}