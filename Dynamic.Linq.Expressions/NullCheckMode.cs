using System;

namespace Dynamic.Linq.Expressions
{
    /// <summary>
    /// Specifies the null check modes for filter expressions.
    /// </summary>
    [Flags]
    public enum NullCheckMode
    {
        /// <summary>
        /// No null checks are prepended to the property path comparison expression.
        /// </summary>
        None = 0,

        /// <summary>
        /// The property path is checked for nulls starting from the first property call from the parameter.
        /// </summary>
        CheckPropertyPath = 1,

        /// <summary>
        /// The parameter itsef is checked for null.
        /// </summary>
        CheckParameter = 2,

        /// <summary>
        /// The parameter and the property path are checked for null.
        /// </summary>
        CheckBoth = CheckPropertyPath | CheckParameter
    }
}
