using System;
using System.Globalization;

namespace Dynamic.Linq.Expressions
{
    /// <summary>
    /// Represents sort setting for sorting data.
    /// </summary>
    public class SortSetting
    {
        /// <summary>
        /// Gets or sets the property path to sort by.
        /// </summary>
        public string PropertyPath { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        public SortOrder SortOrder { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "SortSetting: [PropertyPath = '{0}', SortOrder = {1}]", PropertyPath, SortOrder);
        }
    }
}
