namespace EPR.Common.Functions.Database;

using System.Linq.Expressions;
using System.Reflection;

public static class DelegateReflection
{
    public static ConstructorInfo ConstructorOf<T>(Expression<T> expr) => ((NewExpression)expr.Body).Constructor;

    public static MethodInfo MethodOf<T>(Expression<T> expr) =>
        expr.Body switch
        {
            MethodCallExpression methodCallExpression => methodCallExpression.Method,
            UnaryExpression unaryExpression => ((MethodCallExpression)unaryExpression.Operand).Method,
            _ => throw new NotSupportedException($"Unexpected expression type {expr.Body.GetType()}"),
        };

    public static MethodInfo GenericMethodOf<T>(Expression<T> expr) => MethodOf(expr).GetGenericMethodDefinition();

    public static PropertyInfo PropertyOf<T>(Expression<T> expr) => (PropertyInfo)((MemberExpression)expr.Body).Member;
}