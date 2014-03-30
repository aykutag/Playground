using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NoNulls
{
    public class Option
    {
        public static T Safe<T>(Expression<Func<T>>  input)
        {
            var transform = new NullVisitor().VisitAndConvert(input, "test");

            return transform.Compile()();
        }
    }

    public class NullVisitor : ExpressionVisitor
    {
        private Stack<Expression> expressions = new Stack<Expression>();

        private Expression finalExpression;

        private static Random rand = new Random();

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var result = base.VisitLambda<T>(node);

            return result;
        }

        protected override Expression VisitMember(MemberExpression node)
        {            
            expressions.Push(node);

            if (finalExpression == null)
            {
                finalExpression = node;
            }

            var exp = Visit(node.Expression);

            if (expressions.Count == 0)
            {
                return exp;
            }

            var condition =  BuildIfs(expressions.Pop());

            return Expression.Block(new []{ condition} );
        }

        private LabelTarget Label
        {
            get { return Expression.Label(finalExpression.Type); }
        }

        private Expression BuildIfs(Expression top)
        {
            var returnNull = Expression.Constant(null, finalExpression.Type);//Expression.Return(Expression.Label(), finalExpression.Type));

            var ifNull = Expression.ReferenceEqual(top, Expression.Constant(null));

            var finalReturn = finalExpression;//Expression.Return(Label, finalExpression, finalExpression.Type);

            var condition = Expression.Condition(ifNull, returnNull, expressions.Count == 0 ? finalReturn : BuildIfs(expressions.Pop()));

            return condition;            
        }
    }
}
