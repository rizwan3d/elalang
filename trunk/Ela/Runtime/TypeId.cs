using System;

namespace Ela.Runtime
{
	public sealed class TypeId
	{
		#region Standard
		public readonly static TypeId Integer = new TypeId(ElaTypeCode.Integer);
		public readonly static TypeId Long = new TypeId(ElaTypeCode.Long);
		public readonly static TypeId Single = new TypeId(ElaTypeCode.Single);
		public readonly static TypeId Double = new TypeId(ElaTypeCode.Double);
		public readonly static TypeId Char = new TypeId(ElaTypeCode.Char);
		public readonly static TypeId Boolean = new TypeId(ElaTypeCode.Boolean);
		public readonly static TypeId String = new TypeId(ElaTypeCode.String);
		public readonly static TypeId List = new TypeId(ElaTypeCode.List);
		public readonly static TypeId Tuple = new TypeId(ElaTypeCode.Tuple);
		public readonly static TypeId Record = new TypeId(ElaTypeCode.Record);
		public readonly static TypeId Unit = new TypeId(ElaTypeCode.Unit);
		public readonly static TypeId Lazy = new TypeId(ElaTypeCode.Lazy);
		public readonly static TypeId Function = new TypeId(ElaTypeCode.Function);
		public readonly static TypeId Module = new TypeId(ElaTypeCode.Module);
		public readonly static TypeId Object = new TypeId(ElaTypeCode.Object);
		#endregion


		#region Construction
		internal readonly int Id;

		private TypeId(ElaTypeCode typeCode)
		{
			Id = (Int32)typeCode;
		}

		internal TypeId(int typeId)
		{
			Id = typeId;
		}
		#endregion
	}
}
