using System;

namespace Ela.CodeModel
{
	public enum ElaNodeType
	{
		None = 0,


		ListLiteral = 1,

		ArrayLiteral = 2,

		RecordLiteral = 3,

		TupleLiteral = 4,

		VariantLiteral = 5,
		
		AsyncLiteral = 6,

		LazyLiteral = 7,

		FunctionLiteral = 8,
		


		Assign,

		Unary,

		Binary,

		FunctionCall,

		Indexer,
		
		FieldDeclaration,

		Primitive,

		VariableReference,

		Argument,

		ImportedName,

		FieldReference,

		Placeholder,

		Cast,

		Is,

		Typeof,

		BaseReference,
		
		For,

		While,

		Break,

		Condition,

		Continue,

		Cout,

		TryCatch,

		Return,

		Ignore,

		VariableDeclaration,

		Throw,

		Block,

		Match,

		MatchEntry,

		Yield,

		ModuleInclude,
		
		DefaultPattern,

		VoidPattern,

		ConstructorPattern,

		LiteralPattern,

		BoolPattern,

		VariablePattern,

		HeadTailPattern,

		ValueofPattern,

		ListPattern,

		AndPattern,

		OrPattern,

		SeqPattern,

		ArrayPattern,

		FieldPattern,

		IsPattern,

		AsPattern
	}
}
