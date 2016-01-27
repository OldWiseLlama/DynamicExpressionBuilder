using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamic.Linq.Expressions
{
    public static class ExpressionHelper
    {
        public static readonly Type QueryableType = typeof(Queryable);
        public static readonly Type StringType = typeof(string);
        public const string WhereMethodName = "Where";
        public const string OrderByMethodName = "OrderBy";
        public const string ThenByMethodName = "ThenBy";
        public const string OrderByDescendingMethodName = "OrderByDescending";
        public const string ThenByDescendingMethodName = "ThenByDescending";

        public static readonly MethodInfo GenericEnumerableContains = typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(mi => mi.Name == FilterOperators.ContainsOperator && mi.GetParameters().Count() == 2);

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
    }
}
