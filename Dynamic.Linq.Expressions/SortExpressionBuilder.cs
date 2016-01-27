using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamic.Linq.Expressions
{
    /// <summary>
    /// A class which sorts <see cref="IQueryable{TSource}"/> queries.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements in the queryable this class sorts.</typeparam>
    public class SortExpressionBuilder<TSource>
    {
        /// <summary>
        /// Sorts the specified source queryable based on the specified sort settings.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sortSettings">The sort settings.</param>
        /// <returns>An <see cref="IOrderedQueryable{TSource}"/> which sorts the source queryable.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="sortSettings"/> is empty.</exception>
        public IOrderedQueryable<TSource> Sort(IQueryable<TSource> source, List<SortSetting> sortSettings)
        {
            if (!sortSettings.Any())
            {
                throw new ArgumentException("There must be at least one sort setting.", "sortSettings");
            }
            ParameterExpression parameterExpression = Expression.Parameter(source.ElementType, "item");

            IOrderedQueryable<TSource> sorted = SortFirst(source, sortSettings[0],
                                                          parameterExpression);
            for (var i = 1; i < sortSettings.Count; ++i)
            {
                sorted = SortFollowing(sorted, sortSettings[i], parameterExpression);
            }
            return sorted;
        }

        /// <summary>
        /// Sorts the source query with the first sort setting.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sortSetting">The sort setting.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>The sorted query.</returns>
        protected static IOrderedQueryable<TSource> SortFirst(IQueryable<TSource> source, SortSetting sortSetting, ParameterExpression parameterExpression)
        {
            Type propertyType;
            MemberExpression memberExpression = ExpressionHelper.GetPropertyPathExpression(source.ElementType,
                                                                                           parameterExpression,
                                                                                           sortSetting.PropertyPath,
                                                                                           out propertyType);

            MethodCallExpression orderExpression;
            if (sortSetting.SortOrder == SortOrder.Ascending)
            {
                orderExpression = CreateOrderByExpression(source, memberExpression,
                                                                                 parameterExpression);
            }
            else
            {
                orderExpression = CreateOrderByDescendingExpression(source, memberExpression,
                                                                                 parameterExpression);
            }

            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(orderExpression);
        }

        /// <summary>
        /// Sorts the source ordered query with the specified sort setting.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sortSetting">The sort setting.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>The sorted query.</returns>
        private static IOrderedQueryable<TSource> SortFollowing(IOrderedQueryable<TSource> source, SortSetting sortSetting, ParameterExpression parameterExpression)
        {
            Type propertyType;
            MemberExpression memberExpression = ExpressionHelper.GetPropertyPathExpression(source.ElementType,
                                                                                           parameterExpression,
                                                                                           sortSetting.PropertyPath,
                                                                                           out propertyType);
            MethodCallExpression orderExpression;
            if (sortSetting.SortOrder == SortOrder.Ascending)
            {
                orderExpression = CreateThenByExpression(source, memberExpression,
                                                                                 parameterExpression);
            }
            else
            {
                orderExpression = CreateThenByDescendingExpression(source, memberExpression,
                                                                                 parameterExpression);
            }

            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(orderExpression);
        }

        /// <summary>
        /// Creates the order by expression.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="memberExpression">The member expression.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>A <see cref="MethodCallExpression"/> for OrderBy method.</returns>
        protected static MethodCallExpression CreateOrderByExpression(IQueryable<TSource> source, Expression memberExpression,
                                                                           ParameterExpression parameterExpression)
        {
            MethodCallExpression orderByExpression = Expression.Call(
                ExpressionHelper.QueryableType,
                ExpressionHelper.OrderByMethodName,
                new[] { source.ElementType, memberExpression.Type },
                source.Expression,
                Expression.Lambda(memberExpression,
                                  new[] { parameterExpression })
                );
            return orderByExpression;
        }

        /// <summary>
        /// Creates the order by descending expression.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="memberExpression">The member expression.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>A <see cref="MethodCallExpression"/> for OrderByDescending method.</returns>
        protected static MethodCallExpression CreateOrderByDescendingExpression(IQueryable<TSource> source, Expression memberExpression,
                                                                           ParameterExpression parameterExpression)
        {
            MethodCallExpression orderByExpression = Expression.Call(
                ExpressionHelper.QueryableType,
                ExpressionHelper.OrderByDescendingMethodName,
                new[] { source.ElementType, memberExpression.Type },
                source.Expression,
                Expression.Lambda(memberExpression,
                                  new[] { parameterExpression })
                );
            return orderByExpression;
        }

        /// <summary>
        /// Creates the then by expression.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="memberExpression">The member expression.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>A <see cref="MethodCallExpression"/> for ThenBy method.</returns>
        protected static MethodCallExpression CreateThenByExpression(IOrderedQueryable<TSource> source, Expression memberExpression,
                                                                           ParameterExpression parameterExpression)
        {
            MethodCallExpression orderByExpression = Expression.Call(
                ExpressionHelper.QueryableType,
                ExpressionHelper.ThenByMethodName,
                new[] { source.ElementType, memberExpression.Type },
                source.Expression,
                Expression.Lambda(memberExpression,
                                  new[] { parameterExpression })
                );
            return orderByExpression;
        }

        /// <summary>
        /// Creates the then by descending expression.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="memberExpression">The member expression.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>A <see cref="MethodCallExpression"/> for ThenByDescending method.</returns>
        protected static MethodCallExpression CreateThenByDescendingExpression(IOrderedQueryable<TSource> source, Expression memberExpression,
                                                                           ParameterExpression parameterExpression)
        {
            MethodCallExpression orderByExpression = Expression.Call(
                ExpressionHelper.QueryableType,
                ExpressionHelper.ThenByDescendingMethodName,
                new[] { source.ElementType, memberExpression.Type },
                source.Expression,
                Expression.Lambda(memberExpression,
                                  new[] { parameterExpression })
                );
            return orderByExpression;
        }
    }
}
