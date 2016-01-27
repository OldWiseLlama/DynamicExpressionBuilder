using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamic.Linq.Expressions
{
    /// <summary>
    /// Helper class for creating filtering expressions with <see cref="Expression"/> API.
    /// </summary>
    public static class FilterOperators
    {
        #region Operator names

        /// <summary>
        /// The equals operator.
        /// </summary>
        public const string EqualsOperator = "Equals";

        /// <summary>
        /// The starts with operator.
        /// </summary>
        public const string StartsWithOperator = "StartsWith";

        /// <summary>
        /// The ends with operator.
        /// </summary>
        public const string EndsWithOperator = "EndsWith";

        /// <summary>
        /// The contains operator.
        /// </summary>
        public const string ContainsOperator = "Contains";

        /// <summary>
        /// The less than operator.
        /// </summary>
        public const string LessThanOperator = "LessThan";

        /// <summary>
        /// The greater than operator.
        /// </summary>
        public const string GreaterThanOperator = "GreatedThan";

        /// <summary>
        /// The less than or equal operator.
        /// </summary>
        public const string LessThanOrEqualOperator = "LessThanOrEqual";

        /// <summary>
        /// The greater than or equal operator.
        /// </summary>
        public const string GreaterThanOrEqualOperator = "GreatedThanOrEqual";

        /// <summary>
        /// The not equals operator.
        /// </summary>
        public const string NotEqualsOperator = "NotEquals";

        /// <summary>
        /// Special operator where the comparand value is IEnumerable and the property value is scalar.
        /// This case will be translated into comparand.Contains(propertyValue)
        /// </summary>
        public const string InOperator = "In";

        #endregion

        #region Private fields

        private static readonly IReadOnlyDictionary<string, Func<Expression, Expression, BinaryExpression>> BinaryExpressionFactories
                = new Dictionary<string, Func<Expression, Expression, BinaryExpression>>
                      {
                          {EqualsOperator, Expression.Equal},
                          {NotEqualsOperator, Expression.NotEqual},
                          {LessThanOperator, Expression.LessThan},
                          {GreaterThanOperator, Expression.GreaterThan},
                          {LessThanOrEqualOperator, Expression.LessThanOrEqual},
                          {GreaterThanOrEqualOperator, Expression.GreaterThanOrEqual},
                      };

        #endregion

        /// <summary>
        /// Gets the binary expression factory.
        /// </summary>
        /// <param name="operatorName">Name of the operator.</param>
        /// <returns>A funtion which creates a binary expression matchinf the <paramref name="operatorName"/>.</returns>
        /// <exception cref="System.NotSupportedException">Thrown when the <paramref name="operatorName"/> is not supported.</exception>
        public static Func<Expression, Expression, BinaryExpression> GetBinaryExpressionFactory(string operatorName)
        {
            Func<Expression, Expression, BinaryExpression> value;
            if (BinaryExpressionFactories.TryGetValue(operatorName, out value))
            {
                return value;
            }
            throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, "Operator '{0}' is not a supported binary operator.", operatorName));
        }
    }
}
