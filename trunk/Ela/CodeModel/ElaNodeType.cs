using System;

namespace Ela.CodeModel
{
	public enum ElaNodeType
	{
		None = 0,


		ListLiteral = 1,

		RecordLiteral = 2,

		TupleLiteral = 3,
		
		UnitLiteral = 4,

		FunctionLiteral = 5,

		LazyLiteral = 6,

		VariantLiteral = 7,
		
		

		Builtin,

		Binary,

		FunctionCall,

		Indexer,
		
		FieldDeclaration,

		Primitive,

		VariableReference,

        Attribute,

		Argument,

		FieldReference,

		Placeholder,

		Cast,

		Is,

		Condition,

		Try,

		Binding,

		Raise,

		Block,

		Match,

		MatchEntry,

		Range,

		ModuleInclude,

		Comprehension,

		Generator,


		OtherwiseGuard,
		
		DefaultPattern,

		UnitPattern,

		NilPattern,

		LiteralPattern,

		VariablePattern,

		VariantPattern,

		HeadTailPattern,

		TuplePattern,

		RecordPattern,

		FieldPattern,

		IsPattern,

		AsPattern,

		PatternGroup
	}
}
