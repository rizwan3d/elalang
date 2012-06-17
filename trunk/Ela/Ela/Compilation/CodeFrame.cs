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
        internal FastList<Int32> HandleMap;
		
		private CodeFrame(Op op) : this()
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
            HandleMap = new FastList<Int32>();
            Types = new Dictionary<String,Int32>();
            Classes = new Dictionary<String,ClassData>();
            Instances = new FastList<InstanceData>();
            LateBounds = new FastList<LateBoundSymbol>();
       	}
		#endregion


		#region Nested classes
		private sealed class ReferenceMap : Dictionary<String,ModuleReference>, IReadOnlyMap<String,ModuleReference>
		{
			internal ReferenceMap() { }
			internal ReferenceMap(ReferenceMap copy) : base(copy) { }
		}


        private sealed class AttributeMap : Dictionary<String,String>, IReadOnlyMap<String,String>
        {
            internal AttributeMap() { }
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
            copy.HandleMap = HandleMap.Clone();
            copy.Types = new Dictionary<String,Int32>(Types);
            copy.Classes = new Dictionary<String,ClassData>(Classes);
            copy.Instances = Instances.Clone();
            copy.LateBounds = LateBounds.Clone();
         	return copy;
		}


		public void AddReference(string alias, ModuleReference mr)
		{
			if (_references.ContainsKey(alias))
				_references.Remove(alias);

			_references.Add(alias, mr);
		}


        public override string ToString()
        {
            return File != null ? File.ToString() : "";
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

        internal Dictionary<String,Int32> Types { get; private set; }

        internal Dictionary<String,ClassData> Classes { get; private set; }

        internal FastList<InstanceData> Instances { get; private set; }

        public FastList<LateBoundSymbol> LateBounds { get; private set; }
        #endregion
	}
}