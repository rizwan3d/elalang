using System;
using System.Collections.Generic;
using System.IO;
using Ela.CodeModel;
using Ela.Debug;

namespace Ela.Compilation
{
	public class CodeFrame
	{
		#region Construction
		public static readonly CodeFrame Empty = new CodeFrame(Op.Stop);

		private CodeFrame(Op op)
			: this()
		{
			Ops.Add(op);
			OpData.Add(0);
			Layouts.Add(new MemoryLayout(0, 0, 0));
		}


		internal CodeFrame()
		{
			Layouts = new FastList<MemoryLayout>();
			Ops = new FastList<Op>();
			OpData = new FastList<Int32>();
			Strings = new FastList<String>();
			_references = new ReferenceMap();
			Arguments = new Dictionary<String,Loc>();
			Unresolves = new FastList<UnresolvedSymbol>();
		}
		#endregion


		#region Nested classes
		private sealed class ReferenceMap : Dictionary<String,ModuleReference>, IReadOnlyMap<String,ModuleReference>
		{
			internal ReferenceMap() { }
			internal ReferenceMap(ReferenceMap copy) : base(copy) { }
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
			copy.Symbols = Symbols != null ? Symbols.Clone() : null;
			copy.Arguments = new Dictionary<String,Loc>(Arguments);
			copy.Unresolves = new FastList<UnresolvedSymbol>();
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

		public FileInfo File { get; set; }

		internal FastList<MemoryLayout> Layouts { get; private set; }

		internal FastList<String> Strings { get; private set; }

		internal Dictionary<String,Loc> Arguments { get; private set; }

		internal FastList<UnresolvedSymbol> Unresolves { get; private set; }
		#endregion
	}
}