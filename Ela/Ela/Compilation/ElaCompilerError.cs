﻿using System;

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

		InvalidExpression = 306,

        ElseMissing = 307,

		CastNotSupported = 308,

		InvalidPattern = 309,

		VariableDeclarationInitMissing = 310,

		NameRequireQualified = 311,

		MatchEntryEmptyNoGuard = 312,

        PrivateOnlyGlobal = 313,

		InvalidVariant = 314,

        InvalidBuiltinBinding = 315,

		BindingsAndRestrictedGlobal = 316,

        ClassAlreadyDeclared = 317,

        UnknownClass = 318,

        MemberInvalid = 319,

        MemberNoPatterns = 320,

        MemberNotAll = 321,

        InvalidBuiltinClass = 322,

        InvalidBuiltinClassDefinition = 323,

        NoHindingSameScope = 324,

        InvalidQualident = 325,

        InvalidMemberSignature = 326,

        OnlyBuiltinTypeNoDefinition = 327,

        InvalidProgram = 328,

        TooManyErrors = 329,


        PatternsTooFew = 330,

        PatternsTooMany = 331,

        InvalidMatchEntry = 332,

        TypeMemberExpression = 333,

        TypeMemberNoPatterns = 334,

        HeaderNotConnected = 335,
	}
}