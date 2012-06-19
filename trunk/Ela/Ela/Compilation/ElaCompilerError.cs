using System;

namespace Ela.Compilation
{
	public enum ElaCompilerError
	{
		None = 0,


        PrivateNameInModule = 300,

		UndefinedNameInModule = 301,

		UndefinedName = 302,

		UndefinedType = 303,

		PlaceholderNotValid = 304,

		TypeAlreadyDeclared = 305,

		PatternNotAllArgs = 306,

        ElseMissing = 307,

		CastNotSupported = 308,

		// = 309,

		VariableDeclarationInitMissing = 310,

		ElseGuardNotValid = 311,

		MatchEntryEmptyNoGuard = 312,

        PrivateOnlyGlobal = 313,

		InvalidVariant = 314,

        InvalidBuiltinBinding = 315,

		// = 316,

        ClassAlreadyDeclared = 317,

        UnknownClass = 318,

        MemberNoPatterns = 319,

        // = 320,

        MemberNotAll = 321,

        InvalidBuiltinClass = 322,

        InvalidBuiltinClassDefinition = 323,

        NoHindingSameScope = 324,

        InvalidQualident = 325,

        InvalidMemberSignature = 326,

        OnlyBuiltinTypeNoDefinition = 327,

        InvalidProgram = 328,

        TooManyErrors = 329,
	}
}
