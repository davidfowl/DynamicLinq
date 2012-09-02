namespace DynamicLINQ
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Private implementation details
    /// </summary>
    public static partial class DynamicQueryable
    {
        private static bool Any(IQueryable source, LambdaExpression expression)
        {
            return source.Provider.Execute<bool>(Expression.Call(typeof(Queryable),
                                                                "Any",
                                                                new[] { GetElementType(source), expression.Body.Type },
                                                                source.Expression,
                                                                Expression.Quote(expression)));
        }

        private static int Count(IQueryable source, LambdaExpression expression)
        {
            return source.Provider.Execute<int>(Expression.Call(typeof(Queryable),
                                                                "Count",
                                                                new[] { GetElementType(source) },
                                                                source.Expression, Expression.Quote(expression)));
        }

        private static IQueryable Where(IQueryable source, LambdaExpression expression)
        {
            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable),
                                                               "Where",
                                                               new[] { GetElementType(source) },
                                                               source.Expression,
                                                               Expression.Quote(expression)));
        }


        private static IQueryable OrderBy(IQueryable source, LambdaExpression expression)
        {
            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable),
                                                               "OrderBy",
                                                               new[] { GetElementType(source), expression.Body.Type },
                                                               source.Expression,
                                                               Expression.Quote(expression)));
        }

        private static IQueryable OrderByDescending(IQueryable source, LambdaExpression expression)
        {
            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable),
                                                               "OrderByDescending",
                                                               new[] { GetElementType(source), expression.Body.Type },
                                                               source.Expression,
                                                               Expression.Quote(expression)));
        }

        private static IQueryable<T> Where<T>(IQueryable<T> source, LambdaExpression expression)
        {
            return source.Provider.CreateQuery<T>(Expression.Call(typeof(Queryable),
                                                               "Where",
                                                               new[] { GetElementType(source) },
                                                               source.Expression,
                                                               Expression.Quote(expression)));
        }

        private static dynamic First(IQueryable<dynamic> source, LambdaExpression expression)
        {
            return source.Provider.Execute<dynamic>(Expression.Call(typeof(Queryable),
                                                               "First",
                                                               new[] { GetElementType(source) },
                                                               source.Expression,
                                                               Expression.Quote(expression)));
        }

        private static dynamic FirstOrDefault(IQueryable<dynamic> source, LambdaExpression expression)
        {
            return source.Provider.Execute<dynamic>(Expression.Call(typeof(Queryable),
                                                               "FirstOrDefault",
                                                               new[] { GetElementType(source) },
                                                               source.Expression,
                                                               Expression.Quote(expression)));
        }

        private static IQueryable<TResult> Select<TResult>(IQueryable source, LambdaExpression expression)
        {
            return source.Provider.CreateQuery<TResult>(Expression.Call(typeof(Queryable),
                                                               "Select",
                                                               new[] { GetElementType(source), expression.Body.Type },
                                                               source.Expression,
                                                               Expression.Quote(expression)));
        }

        private static IQueryable<dynamic> OrderBy(IQueryable<dynamic> source, LambdaExpression expression)
        {
            return source.Provider.CreateQuery<dynamic>(Expression.Call(typeof(Queryable),
                                                               "OrderBy",
                                                               new[] { GetElementType(source), expression.Body.Type },
                                                               source.Expression,
                                                               Expression.Quote(expression)));
        }

        private static IQueryable<dynamic> OrderByDescending(IQueryable<dynamic> source, LambdaExpression expression)
        {
            return source.Provider.CreateQuery<dynamic>(Expression.Call(typeof(Queryable),
                                                               "OrderByDescending",
                                                               new[] { GetElementType(source), expression.Body.Type },
                                                               source.Expression,
                                                               Expression.Quote(expression)));
        }

        private static LambdaExpression GetSelectorExpression<TResult>(IQueryable source, Func<dynamic, TResult> selector)
        {
            ParameterExpression parameterExpression = Expression.Parameter(GetElementType(source), selector.Method.GetParameters()[0].Name);
            TResult result = selector(new DynamicExpressionBuilder(parameterExpression));

            if (result == null)
            {
                throw new ArgumentException("Unable to translate expression");
            }

            Expression body = null;
            var properties = typeof(TResult).GetProperties();
            if (properties.Any() && !typeof(TResult).IsPrimitive && typeof(TResult) != typeof(string))
            {
                var members = from property in properties
                              let builder = (DynamicExpressionBuilder)property.GetValue(result, null)
                              select new
                              {
                                  Expression = Expression.Convert(builder.Expression, property.PropertyType),
                                  Member = property
                              };

                body = Expression.New(typeof(TResult).GetConstructors()[0],
                                      members.Select(a => a.Expression),
                                      members.Select(a => a.Member));
            }
            else
            {
                body = (result as DynamicExpressionBuilder).Expression;
            }

            return Expression.Lambda(body, parameterExpression);
        }

        private static LambdaExpression GetExpression(IQueryable source, Func<dynamic, dynamic> expressionBuilder)
        {
            ParameterExpression parameterExpression = Expression.Parameter(GetElementType(source), expressionBuilder.Method.GetParameters()[0].Name);
            DynamicExpressionBuilder dynamicExpression = expressionBuilder(new DynamicExpressionBuilder(parameterExpression));
            Expression body = dynamicExpression.Expression;
            return Expression.Lambda(body, parameterExpression);
        }

        // Walk until we get to the first non object element type
        private static Type GetElementType(IQueryable source)
        {
            Expression expr = source.Expression;
            Type elementType = source.ElementType;
            while (expr.NodeType == ExpressionType.Call &&
                   elementType == typeof(object))
            {
                var call = (MethodCallExpression)expr;
                expr = call.Arguments.First();
                elementType = expr.Type.GetGenericArguments().First();
            }

            return elementType;
        }
    }

    /// <summary>
    /// Public non-generic methods
    /// </summary>
    public static partial class DynamicQueryable
    {
        public static bool DynamicAny(this IQueryable source, Func<dynamic, dynamic> predicate)
        {
            return Any(source, GetExpression(source, predicate));
        }

        public static int DynamicCount(this IQueryable source, Func<dynamic, dynamic> predicate)
        {
            return Count(source, GetExpression(source, predicate));
        }

        public static IQueryable DynamicOrderBy(this IQueryable source, Func<dynamic, dynamic> selector)
        {
            return OrderBy(source, GetExpression(source, selector));
        }

        public static IQueryable DynamicOrderByDescending(this IQueryable source, Func<dynamic, dynamic> selector)
        {
            return OrderByDescending(source, GetExpression(source, selector));
        }

        public static IQueryable<TResult> DynamicSelect<TResult>(this IQueryable source, Func<dynamic, TResult> selector)
        {
            return Select<TResult>(source, GetSelectorExpression(source, selector));
        }
    }

    /// <summary>
    /// Public generic methods (IQ&gt;dynamic&lt;)
    /// </summary>
    public static partial class DynamicQueryable
    {
        public static IQueryable<T> DynamicWhere<T>(this IQueryable<T> source, Func<dynamic, dynamic> predicate)
        {
            return Where<T>(source, GetExpression(source, predicate));
        }

        public static IQueryable<dynamic> DynamicOrderBy(this IQueryable<dynamic> source, Func<dynamic, dynamic> selector)
        {
            return OrderBy(source, GetExpression(source, selector));
        }

        public static IQueryable<dynamic> DynamicOrderByDescending(this IQueryable<dynamic> source, Func<dynamic, dynamic> selector)
        {
            return OrderByDescending(source, GetExpression(source, selector));
        }

        public static dynamic DynamicFirst(this IQueryable<dynamic> source, Func<dynamic, dynamic> predicate)
        {
            return First(source, GetExpression(source, predicate));
        }

        public static dynamic DynamicFirstOrDefault(this IQueryable<dynamic> source, Func<dynamic, dynamic> predicate)
        {
            return FirstOrDefault(source, GetExpression(source, predicate));
        }
    }
}
