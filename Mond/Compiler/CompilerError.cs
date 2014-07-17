﻿namespace Mond.Compiler
{
    static class CompilerError
    {
        public const string UnterminatedString = "Unterminated string";
        public const string UnexpectedEofString = "Unexpected end of file (bad escape sequence)";
        public const string InvalidEscapeSequence = "Invalid escape sequence '{0}'";
        public const string InvalidNumber = "Invalid number";
        public const string UnexpectedCharacter = "Unexpected character '{0}'";
        public const string ExpectedButFound = "Expected {0} but found {1}";

        public const string IdentifierAlreadyDefined = "Identifier '{0}' was previously defined in this scope";
        public const string CantModifyReadonlyVar = "Can not modify '{0}' because it is readonly";
        public const string LeftSideMustBeStorable = "The left side of an assignment must be storable";

        public const string ExpectedCaseOrDefault = "Expected 'case' or 'default' but found {0}";
        public const string ExpectedConstant = "Expected a constant value";
        public const string DuplicateCase = "Duplicate case value";

        public const string BadForLoopInitializer = "For loop initializer can not be block statement";

        public const string FunctionNeverUsed = "Function is never used";

        public const string UnresolvedJump = "Unresolved jump";

        public const string BadStackState = "Bad stack state (compiler bug)";
    }
}