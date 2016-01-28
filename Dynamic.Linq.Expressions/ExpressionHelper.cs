using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamic.Linq.Expressions
{
    /// <summary>
    /// Helper class for creating queries with the <see cref="Expression"/> API.
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// The queryable type.
        /// </summary>
        public static readonly Type QueryableType = typeof(Queryable);

        /// <summary>
        /// The string type
        /// </summary>
        public static readonly Type StringType = typeof(string);

        /// <summary>
        /// The where method name.
        /// </summary>
        public const string WhereMethodName = "Where";

        /// <summary>
        /// The order by method name.
        /// </summary>
        public const string OrderByMethodName = "OrderBy";

        /// <summary>
        /// The then by method name.
        /// </summary>
        public const string ThenByMethodName = "ThenBy";

        /// <summary>
        /// The order by descending method name.
        /// </summary>
        public const string OrderByDescendingMethodName = "OrderByDescending";

        /// <summary>
        /// The then by descending method name.
        /// </summary>
        public const string ThenByDescendingMethodName = "ThenByDescending";

        /// <summary>
        /// The <see cref="MethodInfo"/> for the generic oveload of the Contains extension method in the <see cref="Enumerable"/> class.
        /// </summary>
        public static readonly MethodInfo GenericEnumerableContains = typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(mi => mi.Name == FilterOperators.ContainsOperator && mi.GetParameters().Count() == 2);

        /// <summary>
        /// Gets the a list of <see cref="MemberExpression"/> for each part of the property path where the last item contains the final property path expression.
        /// </summary>
        /// <param name="sourceElementType">Type of the source element.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns>A list of <see cref="MemberExpression"/> for each part of the property path where the last item contains the final property path expression.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the property path does not contain any properties.</exception>
        public static List<MemberExpression> GetPropertyPathExpressions(Type sourceElementType, ParameterExpression parameterExpression, string propertyPath, out Type propertyType)
        {
            var propertyNames = propertyPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (propertyNames.Length < 1)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,
                                                          "Cannot create a member expression from property path '{0}'.",
                                                          propertyPath));
            }

            var pathExpressions = new List<MemberExpression>(propertyNames.Length);

            MemberExpression memberExpression = Expression.PropertyOrField(parameterExpression, propertyNames[0]);
            pathExpressions.Add(memberExpression);
            propertyType = sourceElementType.GetProperty(propertyNames[0]).PropertyType;

            for (var i = 1; i < propertyNames.Length; ++i)
            {
                memberExpression = Expression.PropertyOrField(memberExpression, propertyNames[i]);
                pathExpressions.Add(memberExpression);
                propertyType = propertyType.GetProperty(propertyNames[i]).PropertyType;
            }
            return pathExpressions;
        }

        /// <summary>
        /// Gets the <see cref="MemberExpression"/> corresponding to the property path property path.
        /// </summary>
        /// <param name="sourceElementType">Type of the source element.</param>
        /// <param name="parameterExpression">The parameter expression for the lambda exporession.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="propertyType">Type of the property at the end of the property path.</param>
        /// <returns>The property path <see cref="MemberExpression"/></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static MemberExpression GetPropertyPathExpression(Type sourceElementType, ParameterExpression parameterExpression, string propertyPath, out Type propertyType)
        {
            var propertyNames = propertyPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (propertyNames.Length < 1)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,
                                                          "Cannot create a member expression from property path '{0}'.",
                                                          propertyPath));
            }

            MemberExpression memberExpression = Expression.PropertyOrField(parameterExpression, propertyNames[0]);

            propertyType = sourceElementType.GetProperty(propertyNames[0]).PropertyType;

            for (var i = 1; i < propertyNames.Length; ++i)
            {
                memberExpression = Expression.PropertyOrField(memberExpression, propertyNames[i]);
                propertyType = propertyType.GetProperty(propertyNames[i]).PropertyType;
            }
            return memberExpression;
        }

        /// <summary>
        /// Determines if the specified property path is valid for the specified type.
        /// </summary>
        /// <param name="propertyPath">The property path.</param>
        /// <param name="itemType">Type of the item.</param>
        /// <returns><b>True</b> if the property path is valid; otherwise <b>false</b>.</returns>
        public static bool IsPropertyPathValid(string propertyPath, Type itemType)
        {
            while (true)
            {
                if (String.IsNullOrWhiteSpace(propertyPath))
                {
                    return false;
                }
                int firstSeparatorIndex = propertyPath.IndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                //The property path must end on an identifier
                if (firstSeparatorIndex == propertyPath.Length - 1)
                {
                    return false;
                }
                if (firstSeparatorIndex == -1)
                {
                    return itemType.GetProperty(propertyPath, BindingFlags.Instance | BindingFlags.Public) != null;
                }
                string propertyName = propertyPath.Substring(0, firstSeparatorIndex);
                var propertyInfo = itemType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                if (propertyInfo == null)
                {
                    return false;
                }
                propertyPath = propertyPath.Substring(firstSeparatorIndex + 1);
                itemType = propertyInfo.PropertyType;
            }
        }
    }
}
