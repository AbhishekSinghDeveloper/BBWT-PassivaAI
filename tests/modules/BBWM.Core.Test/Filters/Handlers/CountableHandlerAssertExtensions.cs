using System.Linq.Expressions;

using Xunit;

namespace BBWM.Core.Test.Filters.Handlers;

public static class CountableHandlerAssertExtensions
{
    public static void AssertHandlerExpression<TEntity>(
        this Expression<Func<TEntity, bool>> expression, TEntity entity, bool shouldBe)
    {
        Assert.NotNull(expression);
        var exprCompiled = expression.Compile();
        var exprResult = exprCompiled.Invoke(entity);
        Assert.Equal(shouldBe, exprResult);
    }
}
