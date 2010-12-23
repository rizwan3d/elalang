using System;
using Ela.Parsing;

namespace Ela.CodeModel
{
	public class ElaBuiltinFunction : ElaExpression
	{
		#region Construction
		internal ElaBuiltinFunction(Token tok, ElaNodeType type) : base(tok, type)
		{

		}


		internal ElaBuiltinFunction(Token tok) : base(tok, ElaNodeType.BuiltinFunction)
		{
			
		}


		public ElaBuiltinFunction() : this(null)
		{

		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return (Kind == ElaBuiltinFunctionKind.Operator ? Operator.AsString().PutInBraces() :
				Kind.ToString().ToLower());
		}
		#endregion


		#region Properties
		private ElaBuiltinFunctionKind _kind;
		public ElaBuiltinFunctionKind Kind
		{
			get { return _kind; }
			set
			{
				_kind = value;

				if (_kind == ElaBuiltinFunctionKind.Ignore ||
					_kind == ElaBuiltinFunctionKind.Cout)
					Flags = ElaExpressionFlags.ReturnsUnit;
			}
		}

		public ElaOperator Operator { get; set; }

		internal int ParameterCount 
		{
			get
			{
				return _kind == ElaBuiltinFunctionKind.Operator || 
					_kind == ElaBuiltinFunctionKind.Showf ? 2 : 1;
			}
		}
		#endregion
	}
}