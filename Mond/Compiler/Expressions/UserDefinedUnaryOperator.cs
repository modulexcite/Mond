﻿using System.Collections.Generic;

namespace Mond.Compiler.Expressions
{
    class UserDefinedUnaryOperator : Expression
    {
        public string Operator { get { return Token.Contents; } }
        public Expression Right { get; private set; }

        public UserDefinedUnaryOperator(Token token, Expression right)
            : base(token)
        {
            Right = right;
        }

        public override int Compile(FunctionContext context)
        {
            var stack = 0;

            stack += Right.Compile(context);
            context.Position(Token); // debug info
            stack += context.LoadGlobal();
            stack += context.LoadField(context.String("__ops"));
            stack += context.LoadField(context.String(Operator));
            stack += context.Call(1, new List<ImmediateOperand>());

            CheckStack(stack, 1);
            return stack;
        }

        public override Expression Simplify()
        {
            Right = Right.Simplify();

            return this;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
