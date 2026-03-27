using System;
using System.Linq.Expressions;
using Sprache;

namespace ZParse.Benchmarks.ArithmeticExpressionScenario;

internal static class SpracheArithmeticExpressionParser
{
    private static Parser<ExpressionType> Operator(string op, ExpressionType opType)
    {
        return Sprache.Parse.String(op).Token().Return(opType);
    }

    private static readonly Parser<ExpressionType> Add = Operator("+", ExpressionType.AddChecked);
    private static readonly Parser<ExpressionType> Subtract = Operator(
        "-",
        ExpressionType.SubtractChecked
    );
    private static readonly Parser<ExpressionType> Multiply = Operator(
        "*",
        ExpressionType.MultiplyChecked
    );
    private static readonly Parser<ExpressionType> Divide = Operator("/", ExpressionType.Divide);

    private static readonly Parser<Expression> Constant = Sprache
        .Parse.Decimal.Select(x => Expression.Constant(int.Parse(x)))
        .Named("number");

    private static readonly Parser<Expression> Factor = (
        from lparen in Sprache.Parse.Char('(')
        from expr in Sprache.Parse.Ref(() => Expr)
        from rparen in Sprache.Parse.Char(')')
        select expr
    ).XOr(Constant);

    private static readonly Parser<Expression> Operand = (
        from sign in Sprache.Parse.Char('-')
        from factor in Factor
        select Expression.Negate(factor)
    )
        .XOr(Factor)
        .Named("expression")
        .Token();

    private static readonly Parser<Expression> Term = Sprache.Parse.XChainOperator(
        Multiply.XOr(Divide),
        Operand,
        Expression.MakeBinary
    );

    private static readonly Parser<Expression> Expr = Sprache.Parse.XChainOperator(
        Add.XOr(Subtract),
        Term,
        Expression.MakeBinary
    );

    public static readonly Parser<Expression<Func<int>>> Lambda = Expr.End()
        .Select(body => Expression.Lambda<Func<int>>(body));
}
