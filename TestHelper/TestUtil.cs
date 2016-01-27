using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace TestHelper
{
    /// <summary>
    /// Util class for test helpers
    /// </summary>
    public static class TestUtil
    {
        /// <summary>
        /// Asserts the models.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="skipPropertyNames">The properties to skip.</param>
        public static void AssertModels<TModel>(IList<TModel> expected, IList<TModel> actual, params string[] skipPropertyNames)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            var properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (skipPropertyNames != null && properties.Length > 0)
            {
                properties = properties.Where(p => !skipPropertyNames.Contains(p.Name)).ToArray();
            }
            for (var i = 0; i < expected.Count; ++i)
            {
                foreach (var propertyInfo in properties)
                {
                    Assert.AreEqual(propertyInfo.GetValue(expected[i]), propertyInfo.GetValue(actual[i]),
                                    string.Format(CultureInfo.InvariantCulture, "Values did not match for {0} in the item in index {1}", propertyInfo.Name, i));
                }
            }
        }

        /// <summary>
        /// Asserts the models.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="skipPropertyNames">The properties to skip.</param>
        public static void AssertModel<TModel>(TModel expected, TModel actual, params string[] skipPropertyNames)
        {
            var properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (skipPropertyNames != null && properties.Length > 0)
            {
                properties = properties.Where(p => !skipPropertyNames.Contains(p.Name)).ToArray();
            }

            foreach (var propertyInfo in properties)
            {
                Assert.AreEqual(propertyInfo.GetValue(expected), propertyInfo.GetValue(actual),
                                string.Format(CultureInfo.InvariantCulture, "Values did not match for {0}", propertyInfo.Name));
            }
        }

        /// <summary>
        /// Returns true if the given models are equal, false
        /// otherwise.
        /// </summary>
        public static bool ModelsAreEqual<TModel>(TModel expected, TModel actual, params string[] skipPropertyNames)
        {
            try
            {
                AssertModel(expected, actual, skipPropertyNames);
                return true;
            }
            catch (AssertionException)
            {
                return false;
            }
        }
    }
}
