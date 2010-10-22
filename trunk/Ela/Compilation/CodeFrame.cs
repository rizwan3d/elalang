using System;
using System.Collections.Generic;
using Ela.Debug;
using System.IO;

namespace Ela.Compilation
{
	public class CodeFrame
	{
		#region Nested classes
		private sealed class ReferenceMap : Dictionary<String,ModuleReference>, IReadOnlyMap<String,ModuleReference>
		{
			internal ReferenceMap() { }
			internal ReferenceMap(ReferenceMap copy) : base(copy) { }
		}
		#endregion


		#region Construction
		public static readonly CodeFrame Empty = new CodeFrame(Op.Term);

		private CodeFrame(Op op) : this()
		{
			Ops.Add(op);
			OpData.Add(0);
			Layouts.Add(new MemoryLayout(0, 0));
		}


		internal CodeFrame()
		{
			Layouts = new FastList<MemoryLayout>();
			Ops = new FastList<Op>();
			OpData = new FastList<Int32>();
			Strings = new FastList<String>();
			_references = new ReferenceMap();
			Pervasives = new Dictionary<String,Int32>();
			Variants = new Dictionary<String,Int32>();
		}
		#endregion


		#region Methods
		public CodeFrame Clone()
		{
			var copy = new CodeFrame();
			copy.Layouts = Layouts.Clone();
			copy.Strings = Strings.Clone();
			copy.GlobalScope = GlobalScope.Clone();
			copy.Ops = Ops.Clone();
			copy.OpData = OpData.Clone();
			copy._references = new ReferenceMap(_references);
			copy.Pervasives = new Dictionary<String,Int32>(Pervasives);
			copy.Symbols = Symbols != null ? Symbols.Clone() : null;
			copy.Variants = new Dictionary<String,Int32>(Variants);
			return copy;
		}


		public void AddReference(string alias, ModuleReference mr)
		{
			if (_references.ContainsKey(alias))
				_references.Remove(alias);

			_references.Add(alias, mr);
		}
		#endregion


		#region Properties
		private ReferenceMap _references;
		public IReadOnlyMap<String,ModuleReference> References 
		{ 
			get { return _references; } 
		}

		public FastList<Op> Ops { get; private set; }

		public FastList<Int32> OpData { get; private set; }

		public DebugInfo Symbols { get; internal set; }

		public Scope GlobalScope { get; internal set; }
		
		public FileInfo File { get; internal set; }
		
		internal Dictionary<String,Int32> Variants { get; private set; }

		internal FastList<MemoryLayout> Layouts { get; private set; }

		internal FastList<String> Strings { get; private set; }

		internal Dictionary<String,Int32> Pervasives { get; private set; }		
		#endregion
	}
}