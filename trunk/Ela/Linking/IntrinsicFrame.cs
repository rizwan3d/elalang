using System;
using Ela.Compilation;
using Ela.Runtime;

namespace Ela.Linking
{
	public sealed class IntrinsicFrame : CodeFrame
	{
		#region Construction
		private ForeignModule module;

		internal IntrinsicFrame(ElaValue[] mem, FastList<BinaryOverload> binaryOverloads, FastList<UnaryOverload> unaryOverloads, FastList<TernaryOverload> ternaryOverloads, ForeignModule module)
		{
			Memory = mem;
			this.module = module;
			BinaryOverloads = binaryOverloads;
			UnaryOverloads = unaryOverloads;
			TernaryOverloads = ternaryOverloads;
		}
		#endregion


		#region Methods
		public override void RegisterTypes(TypeRegistrator registrator)
		{
			module.RegisterTypes(registrator);
		}
		#endregion


		#region Properties
		internal ElaValue[] Memory { get; private set; }

		internal FastList<TernaryOverload> TernaryOverloads { get; private set; }
		
		internal FastList<BinaryOverload> BinaryOverloads { get; private set; }

		internal FastList<UnaryOverload> UnaryOverloads { get; private set; }
		#endregion
	}
}
