using System.Linq;
using System.Linq.Expressions;

namespace DynamicLINQ
{
    public static class QueryProviderExtensions
    {
        /// <summary>
        /// Constructs an System.Linq.IOrderedQueryable object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="call">The call.</param>
        /// <returns></returns>
        public static IOrderedQueryable CreateOrderedQuery(this IQueryProvider provider, MethodCallExpression call)
        {
            return (IOrderedQueryable) provider.CreateQuery(call);
        }

        /// <summary>
        /// Constructs an System.Linq.IOrderedQueryable<typeparam name="T"></typeparam> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider">The provider.</param>
        /// <param name="call">The call.</param>
        /// <returns></returns>
        public static IOrderedQueryable<T> CreateOrderedQuery<T>(this IQueryProvider provider, MethodCallExpression call)
        {
            return (IOrderedQueryable<T>)provider.CreateQuery(call);
        }
    }
}