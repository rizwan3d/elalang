using System;

namespace Ela.CodeModel
{
	public enum ElaNodeType
	{
		None = 0,


		ListLiteral = 1,

		// = 2,

		RecordLiteral = 3,

		TupleLiteral = 4,
		
		UnitLiteral = 5,

		FunctionLiteral = 6,

		LazyLiteral = 7,

		VariantLiteral = 8,
		
		

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

		BaseReference,
		
		For,

		While,

		Break,

		Condition,

		Continue,

		Try,

		Return,

		Binding,

		Raise,

		Block,

		Match,

		MatchEntry,

		Range,

		ModuleInclude,

		BuiltinFunction,

		CustomOperator,

		Comprehension,


		OtherwiseGuard,
		
		DefaultPattern,

		UnitPattern,

		NilPattern,

		LiteralPattern,

		VariablePattern,

		VariantPattern,

		HeadTailPattern,

		SeqPattern,

		RecordPattern,

		FieldPattern,

		IsPattern,

		CastPattern,

		AsPattern,

		PatternGroup
	}
}
