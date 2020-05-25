using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Expressionist 
{
    public static class Visitor
    {
        public static Func<Expression, T> Make<T>(Func<Expression, Func<Expression, T>, T> visit) => new Visitor<T>(visit).Apply;
    }

    public class Visitor<T> : ExpressionVisitor
    {
        readonly Func<Expression, Func<Expression, T>, T> visit;

        public Visitor(Func<Expression, Func<Expression, T>, T> visit) => this.visit = visit;

        readonly Stack<T> results = new Stack<T>();

        public override Expression Visit(Expression node)
        {
            results.Push(Apply(node));
            return node;
        }

        public T Apply(Expression node) =>
            visit(node, nextNode =>
            {
                // if this is another node, then visit it straight away
                if (!ReferenceEquals(node, nextNode)) return Apply(nextNode);

                // else if we try to visit the same node again,
                // dive into the ExpressionVisitor to visit children instead
                base.Visit(nextNode);

                if (results.Count > 1)
                {
                    throw new InvalidOperationException(Indentional.Indentional.Indent($@"
                        Node of type '{nextNode}' yielded {results.Count} results.
                        
                        Please implement a visitor for this node type and explicitly return a result."));
                }

                return results.Pop();
            });
    }
}