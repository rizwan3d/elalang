using System;
using System.Text;
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
		internal override void ToString(StringBuilder sb, Fmt fmt)
		{
			if (Kind == ElaBuiltinFunctionKind.Operator)
			{
				if ((fmt.Flags & FmtFlags.NoParen) != FmtFlags.NoParen)
					sb.Append('(');

				sb.Append(Format.OperatorAsString(Operator));

				if ((fmt.Flags & FmtFlags.NoParen) != FmtFlags.NoParen)
					sb.Append(')');
			}
			else
				sb.Append(Kind.ToString().ToLower());
		}
		#endregion


		#region Properties
		private ElaBuiltinFunctionKind _kind;
		public ElaBuiltinFunctionKind Kind
		{
			get { return _kind; }
			set { _kind = value; }
		}

		public ElaOperator Operator { get; set; }

		internal int ParameterCount 
		{
			get
			{
				return _kind == ElaBuiltinFunctionKind.Operator && Operator != ElaOperator.BitwiseNot && Operator != ElaOperator.Negate || 
					_kind == ElaBuiltinFunctionKind.Showf ||
                    _kind == ElaBuiltinFunctionKind.Ref ? 2 : 1;
			}
		}
		#endregion
	}
}