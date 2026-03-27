using System;
using System.Linq.Expressions;
using ZParse.Parsers;

namespace ZParse.Tests.ArithmeticExpressionScenario;

internal static class ArithmeticExpressionParser
{
    private static TokenListParser<ArithmeticExpressionToken, ExpressionType> Operator(
        ArithmeticExpressionToken op,
        ExpressionType opType
    )
    {
        return Token.EqualTo(op).Value(opType);
    }

    private static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Add =
        Operator(ArithmeticExpressionToken.Plus, ExpressionType.AddChecked);
    private static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Subtract =
        Operator(ArithmeticExpressionToken.Minus, ExpressionType.SubtractChecked);
    private static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Multiply =
        Operator(ArithmeticExpressionToken.Times, ExpressionType.MultiplyChecked);
    private static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Divide =
        Operator(ArithmeticExpressionToken.Divide, ExpressionType.Divide);

    private static readonly TokenListParser<ArithmeticExpressionToken, Expression> Constant = Token
        .EqualTo(ArithmeticExpressionToken.Number)
        .Apply(Numerics.IntegerInt32)
        .Select(Expression (n) => Expression.Constant(n));

    private static readonly TokenListParser<ArithmeticExpressionToken, Expression> Factor = (
        from lparen in Token.EqualTo(ArithmeticExpressionToken.LParen)
        from expr in Parse.Ref(() => Expr!)
        from rparen in Token.EqualTo(ArithmeticExpressionToken.RParen)
        select expr
    ).Or(Constant);

    private static readonly TokenListParser<ArithmeticExpressionToken, Expression> Operand = (
        from sign in Token.EqualTo(ArithmeticExpressionToken.Minus)
        from factor in Factor
        select (Expression)Expression.Negate(factor)
    )
        .Or(Factor)
        .Named("expression");

    private static readonly TokenListParser<ArithmeticExpressionToken, Expression> Term =
        Parse.Chain(Multiply.Or(Divide), Operand, Expression.MakeBinary);

    private static readonly TokenListParser<ArithmeticExpressionToken, Expression> Expr =
        Parse.Chain(Add.Or(Subtract), Term, Expression.MakeBinary);

    public static readonly TokenListParser<
        ArithmeticExpressionToken,
        Expression<Func<int>>
    > Lambda = Expr.AtEnd().Select(body => Expression.Lambda<Func<int>>(body));
}
