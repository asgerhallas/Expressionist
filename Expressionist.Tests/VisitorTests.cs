using System;
using System.Linq.Expressions;
using Shouldly;
using Xunit;

namespace Expressionist.Tests
{
    public class VisitorTests
    {
        [Fact]
        public void GetMemberName()
        {
            var result = Visitor.Make<string>((ex, next) => ex switch
            {
                MemberExpression member => member.Member.Name,
                _ => next(ex)
            })((Expression<Func<MyClass, int>>)(x => x.X));

            result.ShouldBe("X");
        }

        [Fact]
        public void GetMemberChain_FailsBecauseItVisitsLambdaParam()
        {
            Should.Throw<InvalidOperationException>(() => Visitor.Make<string>((ex, next) => ex switch
            {
                ParameterExpression param => param.Name,
                MemberExpression member => $"{next(member.Expression)}/{member.Member.Name}",
                MethodCallExpression method => $"{next(method.Object)}/{method.Method.Name}",
                _ => next(ex)
            })((Expression<Func<MyClass, string>>)(x => x.X.ToString())));
        }

        [Fact]
        public void GetMemberChain()
        {
            var result = Visitor.Make<string>((ex, next) => ex switch
            {
                LambdaExpression l => next(l.Body),
                ParameterExpression param => param.Name,
                MemberExpression member => $"{next(member.Expression)}/{member.Member.Name}",
                MethodCallExpression method => $"{next(method.Object)}/{method.Method.Name}",
                _ => next(ex)
            })((Expression<Func<MyClass, string>>)(x => x.X.ToString()));

            result.ShouldBe("x/X/ToString");
        }

        static Expression<Func<T1, T2>> F<T1, T2>(Expression<Func<T1, T2>> func) => func;

        public class MyClass
        {
            public int X { get; set; }
        }
    }

}
