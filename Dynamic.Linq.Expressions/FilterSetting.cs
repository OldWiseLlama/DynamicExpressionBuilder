using System.Collections.Generic;

namespace Dynamic.Linq.Expressions
{
    /// <summary>
    /// Represents a filter setting for a property
    /// </summary>
    public class FilterSetting
    {
        /// <summary>
        /// Gets or sets the property path to the property to filter with.
        /// </summary>
        public string PropertyPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the operator.
        /// </summary>
        /// <value>
        /// The name of the operator.
        /// </value>
        public string OperatorName { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the OR-connected filters.
        /// </summary>
        /// <remarks>
        /// When these are specified the <c>PropertyPath</c>, <c>OperatorName</c> and <c>Value</c> properties are ignored
        /// </remarks>
        /// <value>
        /// The OR-connected filters.
        /// </value>
        public IEnumerable<FilterSetting> OrConnectedFilters { get; set; }
    }
}
