using System;
using System.Collections.Generic;
using Ela.CodeModel;

namespace Ela.Compilation
{
	internal sealed class CodeWriter
	{
		#region Construction
		private FastList<Op> ops;
		private FastList<Int32> opData;
		private FastList<Int32> labels;
		private FastList<Int32> fixups;

		internal CodeWriter(FastList<Op> ops, FastList<Int32> opData)
		{
			this.ops = ops;
			this.opData = opData;
			labels = new FastList<Int32>();
			fixups = new FastList<Int32>();
		}
		#endregion


		#region Methods
		internal void CompileOpList()
		{
			foreach (var i in fixups)
			{
				opData[i] = labels[opData[i]];
			}

			fixups.Clear();
			labels.Clear();
		}


		internal Op GetOpCode(int offset)
		{
			return ops[offset];
		}


		internal int GetOpCodeData(int offset)
		{
			return opData[offset];
		}


		internal Label DefineLabel()
		{
			var lab = new Label(labels.Count);
			labels.Add(Label.EmptyLabel);
			return lab;
		}


		internal void MarkLabel(Label label)
		{
			labels[label.GetIndex()] = ops.Count;
		}


		internal void Emit(Op op, Label label)
		{
			if (!label.IsEmpty())
			{
				fixups.Add(ops.Count);
				Emit(op, label.GetIndex());
			}
			else
				Emit(op);
		}


		internal void Emit(Op op, int data)
		{
			ops.Add(op);
			opData.Add(data);
		}


		internal void Emit(Op op)
		{
			ops.Add(op);
			opData.Add(0);
		}
		#endregion


		#region Properties
		internal int Offset
		{
			get { return ops.Count; }
		}
		#endregion
	}
}