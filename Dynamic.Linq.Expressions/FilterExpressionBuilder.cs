using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamic.Linq.Expressions
{
    /// <summary>
    /// A class which can filter <see cref="IQueryable{TSource}"/> queries.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements source query.</typeparam>
    public class FilterExpressionBuilder<TSource>
    {
        /// <summary>
        /// Filters the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filterSettings">The filter settings.</param>
        /// <param name="nullCheckMode">The null check mode. Default value is <see cref="NullCheckMode.CheckBoth"/>.</param>
        /// <returns>The query where the filters are applied.</returns>
        public IQueryable<TSource> Filter(IQueryable<TSource> source, IEnumerable<FilterSetting> filterSettings, NullCheckMode nullCheckMode = NullCheckMode.CheckBoth)
        {
            if (filterSettings == null)
            {
                return source;
            }

            source = filterSettings.Aggregate(source, (query, filter) => Apply(query, filter, nullCheckMode));

            return source;
        }

        /// <summary>
        /// Applies the filter to the source query.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filterSetting">The filter setting.</param>
        /// <param name="nullCheckMode">The null check mode.</param>
        /// <returns>A query where the filter is applied.</returns>
        protected virtual IQueryable<TSource> Apply(IQueryable<TSource> source, FilterSetting filterSetting, NullCheckMode nullCheckMode)
        {
            ParameterExpression parameterExpression = Expression.Parameter(source.ElementType, "item");

            Expression filterExpression;
            List<FilterSetting> orConnectedFilters;
            if (HasOrFilters(filterSetting, out orConnectedFilters))
            {
                //It only makes sense to Or if there are more than 1 filters.
                if (orConnectedFilters.Count > 1)
                {
                    filterExpression = GetOrExpression(parameterExpression, source.ElementType,
                                                       orConnectedFilters, nullCheckMode);
                }
                else
                {
                    filterExpression = GetSingleOperatorExpression(source.ElementType, filterSetting.OrConnectedFilters.First(), parameterExpression, nullCheckMode);
                }

            }
            else
            {
                filterExpression = GetSingleOperatorExpression(source.ElementType, filterSetting, parameterExpression, nullCheckMode);
            }

            var whereExpression = CreateWhereExpression(source, filterExpression, parameterExpression);
            source = source.Provider.CreateQuery<TSource>(whereExpression);
            return source;
        }

        /// <summary>
        /// Determines if the filter setting has an OR filter.
        /// </summary>
        /// <param name="filterSetting">The filter setting.</param>
        /// <param name="orConnectedFilters">The or connected filters.</param>
        /// <returns><b>True</b> if the filter setting has an OR filter; otherwise <b>false</b>.</returns>
        protected static bool HasOrFilters(FilterSetting filterSetting, out List<FilterSetting> orConnectedFilters)
        {
            if (filterSetting.OrConnectedFilters == null || !filterSetting.OrConnectedFilters.Any())
            {
                orConnectedFilters = null;
                return false;
            }
            orConnectedFilters = new List<FilterSetting>();

            AppendOrFilters(filterSetting, orConnectedFilters);
            return true;
        }

        private static void AppendOrFilters(FilterSetting filterSetting, List<FilterSetting> orConnectedFilters)
        {
            var allOrFilters = new List<FilterSetting>();
            if (filterSetting.OrConnectedFilters != null && filterSetting.OrConnectedFilters.Any())
            {
                foreach (var orFilter in filterSetting.OrConnectedFilters)
                {
                    List<FilterSetting> innerOrFilters;
                    if (HasOrFilters(orFilter, out innerOrFilters))
                    {
                        foreach (var innerOrFilter in innerOrFilters)
                        {
                            AppendOrFilters(innerOrFilter, allOrFilters);
                        }
                    }
                    else
                    {
                        allOrFilters.Add(orFilter);
                    }
                }
            }
            else
            {
                allOrFilters.Add(filterSetting);
            }
            orConnectedFilters.AddRange(allOrFilters);
        }

        /// <summary>
        /// Gets a single operator expression.
        /// </summary>
        /// <param name="sourceElementType">Type of the source element.</param>
        /// <param name="filterSetting">The filter setting.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="nullCheckMode">The null check mode.</param>
        /// <returns>An expression for the filter setting which can be applied to the source query.</returns>
        protected virtual Expression GetSingleOperatorExpression(Type sourceElementType, FilterSetting filterSetting,
                                                       ParameterExpression parameterExpression, NullCheckMode nullCheckMode)
        {
            var operatorName = filterSetting.OperatorName;
            object comparandValue = filterSetting.Value;

            Type propertyType;

            List<MemberExpression> propertyPathExpressions = GetPropertyPathExpressions(sourceElementType, parameterExpression,
                                                                                filterSetting.PropertyPath,
                                                                                out propertyType);

            Expression operatorExpression = GetOperatorExpression(parameterExpression, operatorName, propertyPathExpressions,
                                                                  comparandValue, propertyType, nullCheckMode);
            return operatorExpression;
        }

        /// <summary>
        /// Gets the a list of <see cref="MemberExpression"/> for each part of the property path where the last item contains the final property path expression.
        /// </summary>
        /// <param name="sourceElementType">Type of the source element.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns>A list of <see cref="MemberExpression"/> for each part of the property path where the last item contains the final property path expression.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the property path does not contain any properties.</exception>
        protected virtual List<MemberExpression> GetPropertyPathExpressions(Type sourceElementType, ParameterExpression parameterExpression, string propertyPath, out Type propertyType)
        {
            return ExpressionHelper.GetPropertyPathExpressions(sourceElementType, parameterExpression, propertyPath, out propertyType);
        }

        /// <summary>
        /// Gets an OR expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="sourceElementType">Type of the source element.</param>
        /// <param name="orConnectedFilters">The or connected filters.</param>
        /// <param name="nullCheckMode">The null check mode.</param>
        /// <returns>An expression which as all of the specified filters OR connected.</returns>
        protected Expression GetOrExpression(ParameterExpression parameterExpression, Type sourceElementType, IEnumerable<FilterSetting> orConnectedFilters, NullCheckMode nullCheckMode)
        {
            var operatorExpressions = new List<Expression>();

            foreach (var filterSetting in orConnectedFilters)
            {
                Expression operatorExpression = GetSingleOperatorExpression(sourceElementType, filterSetting,
                                                                            parameterExpression, nullCheckMode);
                operatorExpressions.Add(operatorExpression);
            }
            Expression orExpression = Expression.Or(operatorExpressions[0], operatorExpressions[1]);
            for (var i = 2; i < operatorExpressions.Count; ++i)
            {
                orExpression = Expression.Or(orExpression, operatorExpressions[i]);
            }

            return orExpression;
        }

        /// <summary>
        /// Gets an operator expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="operatorName">Name of the operator.</param>
        /// <param name="propertyPathExpressions">The property path expressions.</param>
        /// <param name="comparandValue">The comparand value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="nullCheckMode">The null check mode.</param>
        /// <returns>An expression which applies an operator to the property path with the specified value.</returns>
        protected virtual Expression GetOperatorExpression(ParameterExpression parameterExpression, string operatorName, List<MemberExpression> propertyPathExpressions, object comparandValue, Type propertyType, NullCheckMode nullCheckMode)
        {

            MemberExpression propertyPathExpression = propertyPathExpressions[propertyPathExpressions.Count - 1];
            Expression operatorExpression;
            bool checkLastPropertyForNull = false;
            switch (operatorName)
            {
                case FilterOperators.EqualsOperator:
                case FilterOperators.NotEqualsOperator:
                case FilterOperators.LessThanOperator:
                case FilterOperators.GreaterThanOperator:
                case FilterOperators.LessThanOrEqualOperator:
                case FilterOperators.GreaterThanOrEqualOperator:
                    operatorExpression = GetBinaryExpression(propertyPathExpression, comparandValue, propertyType,
                                               FilterOperators.GetBinaryExpressionFactory(operatorName),
                                               parameterExpression);
                    break;

                case FilterOperators.StartsWithOperator:
                case FilterOperators.EndsWithOperator:
                case FilterOperators.ContainsOperator:
                    operatorExpression = GetMethodCallExpression(propertyPathExpression, comparandValue, propertyType, operatorName,
                                                   parameterExpression);
                    checkLastPropertyForNull = true;
                    break;
                case FilterOperators.InOperator:
                    operatorExpression = GetInOperatorExpression(propertyPathExpression, comparandValue, propertyType, parameterExpression);
                    checkLastPropertyForNull = true;
                    break;
                default:
                    operatorExpression = GetNonStandardOperator(propertyPathExpression, comparandValue, propertyType, operatorName,
                                                  parameterExpression);
                    break;
            }

            return AppendPathNullCheckExpression(propertyPathExpressions, checkLastPropertyForNull, operatorExpression, parameterExpression, nullCheckMode);
        }

        /// <summary>
        /// Appends the path null check expression.
        /// </summary>
        /// <param name="propertyPathExpressions">The property path expressions.</param>
        /// <param name="checkLast">if set to <c>true</c> [check last].</param>
        /// <param name="operatorExpression">The operator expression.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="nullCheckMode">The null check mode.</param>
        /// <returns>An expression which checks the property path for nulls first before applying the operato expression.</returns>
        protected Expression AppendPathNullCheckExpression(List<MemberExpression> propertyPathExpressions, bool checkLast, Expression operatorExpression, ParameterExpression parameterExpression, NullCheckMode nullCheckMode)
        {
            var expressionsToAnd = new List<Expression>();
            if (nullCheckMode.HasFlag(NullCheckMode.CheckParameter))
            {
                expressionsToAnd.Add(Expression.NotEqual(parameterExpression, Expression.Constant(null, parameterExpression.Type)));
            }
            if (nullCheckMode.HasFlag(NullCheckMode.CheckPropertyPath))
            {
                int upperBound = checkLast ? propertyPathExpressions.Count : propertyPathExpressions.Count - 1;
                for (var i = 0; i < upperBound; ++i)
                {
                    MemberExpression memberExpression = propertyPathExpressions[i];
                    var nullConstant = Expression.Constant(null, memberExpression.Type);
                    var notNull = Expression.NotEqual(memberExpression, nullConstant);
                    expressionsToAnd.Add(notNull);
                }
            }
            expressionsToAnd.Add(operatorExpression);
            if (expressionsToAnd.Count > 1)
            {
                BinaryExpression and = Expression.AndAlso(expressionsToAnd[0], expressionsToAnd[1]);
                for (var i = 2; i < expressionsToAnd.Count; ++i)
                {
                    and = Expression.AndAlso(and, expressionsToAnd[i]);
                }
                return and;
            }
            else
            {
                return expressionsToAnd.Count == 1 ? expressionsToAnd[0] : null;
            }
        }

        /// <summary>
        /// Gets an IN operator expression.
        /// </summary>
        /// <param name="propertyPathExpression">The property path expression.</param>
        /// <param name="comparandValue">The comparand value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the comparadn value is <b>null</b>
        /// OR when the operator cannot be applied.
        /// </exception>
        protected virtual Expression GetInOperatorExpression(MemberExpression propertyPathExpression, object comparandValue, Type propertyType, ParameterExpression parameterExpression)
        {
            if (comparandValue == null)
            {
                throw new ArgumentException("In operator cannot be applied with null comparand value.");
            }
            Type comparandType = comparandValue.GetType();
            if (comparandType == ExpressionHelper.StringType)
            {
                if (propertyType != ExpressionHelper.StringType)
                {
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Cannot apply In operator to property '{0}'({1}) with string comparand.", propertyPathExpression, propertyType.Name));
                }
                return GetStringContainsMethodCallExpression(propertyPathExpression, (string)comparandValue, parameterExpression);
            }
            if (comparandValue is IEnumerable)
            {
                return GetEnumerableContainsMethodCallExpression(propertyPathExpression, comparandValue, propertyType, parameterExpression);
            }
            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,
                                                      "In operator cannot be applied comparand type {0}.", comparandType));
        }

        /// <summary>
        /// Gets the enumerable contains method call expression.
        /// </summary>
        /// <param name="propertyPathExpression">The property path expression.</param>
        /// <param name="comparandValue">The comparand value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>A <see cref="MethodCallExpression"/> for which corresponds to the Contains extension method in the <see cref="Enumerable"/> class.</returns>
        protected virtual MethodCallExpression GetEnumerableContainsMethodCallExpression(MemberExpression propertyPathExpression,
                                                                                               object comparandValue,
                                                                                               Type propertyType,
                                                                                               ParameterExpression
                                                                                                   parameterExpression)
        {
            Type genericEnumerableType = typeof(IEnumerable<>).MakeGenericType(propertyType);

            Expression comparandExpression = Expression.Constant(comparandValue, genericEnumerableType);
            MethodInfo enumerableContainsMethod = ExpressionHelper.GenericEnumerableContains.MakeGenericMethod(new[] { propertyType });
            MethodCallExpression methodCall = Expression.Call(null,
                                                              enumerableContainsMethod,
                                                              new[] { comparandExpression, propertyPathExpression });
            return methodCall;
        }

        /// <summary>
        /// Gets the string contains method call expression.
        /// </summary>
        /// <param name="propertyPathExpression">The property path expression.</param>
        /// <param name="comparandValue">The comparand value.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>A <see cref="MethodCallExpression"/> for which corresponds to the Contains method in the <see cref="String"/> class.</returns>
        protected virtual MethodCallExpression GetStringContainsMethodCallExpression(MemberExpression propertyPathExpression,
                                                                                           string comparandValue,
                                                                                           ParameterExpression
                                                                                               parameterExpression)
        {
            //right expression of the condition {left} {condition operation} {right}
            Expression comparandExpression = Expression.Constant(comparandValue, ExpressionHelper.StringType);

            MethodCallExpression methodCall = Expression.Call(comparandExpression,
                                                              ExpressionHelper.StringType.GetMethod("Contains",
                                                                                   new[] { ExpressionHelper.StringType }),
                                                              new Expression[] { propertyPathExpression });
            return methodCall;
        }

        /// <summary>
        /// Gets the method call expression.
        /// </summary>
        /// <param name="propertyPathExpression">The property path expression.</param>
        /// <param name="comparandValue">The comparand value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>A <see cref="MethodCallExpression"/> for the specified method.</returns>
        protected virtual MethodCallExpression GetMethodCallExpression(MemberExpression propertyPathExpression, object comparandValue,
                                                                             Type propertyType, string methodName,
                                                                             ParameterExpression parameterExpression)
        {
            Expression right = Expression.Constant(comparandValue, propertyType);
            MethodCallExpression methodCall = Expression.Call(propertyPathExpression,
                                                              propertyType.GetMethod(methodName,
                                                                                     new[] { propertyType }),
                                                              new[] { right });
            return methodCall;
        }

        /// <summary>
        /// Extends the support for new operators when implemented in a derived class.
        /// </summary>
        /// <param name="propertyPathExpression">The property path expression.</param>
        /// <param name="comparandValue">The comparand value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="operatorName">Name of the operator.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>
        /// Expression which applies the operator to the <paramref name="parameterExpression" /> with the <paramref name="comparandValue" />.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Default implementation does not support non standard operators.</exception>
        protected virtual Expression GetNonStandardOperator(MemberExpression propertyPathExpression, object comparandValue, Type propertyType, string operatorName, ParameterExpression parameterExpression)
        {
            throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture,
                                                          "Operator '{0}' is not supported. ", operatorName));
        }

        /// <summary>
        /// Gets a binary expression.
        /// </summary>
        /// <param name="propertyPathExpression">The property path expression.</param>
        /// <param name="comparandValue">The comparand value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="binaryExpressionFactory">The binary expression factory.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>A <see cref="BinaryExpression"/>.</returns>
        protected virtual BinaryExpression GetBinaryExpression(MemberExpression propertyPathExpression, object comparandValue,
                                                                     Type propertyType, Func<Expression, Expression, BinaryExpression> binaryExpressionFactory,
                                                                     ParameterExpression parameterExpression)
        {
            ConstantExpression right = Expression.Constant(comparandValue, propertyType);
            BinaryExpression binaryExpression = binaryExpressionFactory(propertyPathExpression, right);
            return binaryExpression;
        }

        /// <summary>
        /// Creates the where expression.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="conditionExpression">The condition expression.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>A <see cref="MethodCallExpression"/> which corresponds to the Where extension method in the <see cref="Queryable"/> class.</returns>
        protected static MethodCallExpression CreateWhereExpression(IQueryable<TSource> source, Expression conditionExpression,
                                                                           ParameterExpression parameterExpression)
        {
            MethodCallExpression whereExpression = Expression.Call(
                ExpressionHelper.QueryableType,
                ExpressionHelper.WhereMethodName,
                new[] { source.ElementType },
                source.Expression,
                Expression.Lambda<Func<TSource, bool>>(conditionExpression,
                                                       new[] { parameterExpression })
                );
            return whereExpression;
        }
    }
}
