using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
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
        /// <param name="useActualTypeIfPossible">if set to <b>true</b> uses the actual type of the objects if possible.</param>
        /// <param name="skipPropertyNames">The properties to skip. Names without . will be global for all types. Names with Type prefix sync as SomeClass.SomeProperty will be specific to that type.</param>
        public static void AssertModels<TModel>(IList<TModel> expected, IList<TModel> actual, bool useActualTypeIfPossible = true, params string[] skipPropertyNames)
        {
            var type = typeof(TModel);
            AssertModels(type, (IList)expected, (IList)actual, useActualTypeIfPossible, skipPropertyNames);
        }

        /// <summary>
        /// Asserts the models.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="useActualTypeIfPossible">if set to <b>true</b> uses the actual type of the objects if possible.</param>
        /// <param name="skipPropertyNames">The properties to skip. Names without . will be global for all types. Names with Type prefix sync as SomeClass.SomeProperty will be specific to that type.</param>
        public static void AssertModels(Type modelType, IList expected, IList actual, bool useActualTypeIfPossible = true, params string[] skipPropertyNames)
        {
            Assert.AreEqual(expected.Count, actual.Count, "Actual collection has different number of items than the expected collection.");

            for (var i = 0; i < expected.Count; ++i)
            {
                try
                {
                    AssertModel(modelType, expected[i], actual[i], useActualTypeIfPossible, skipPropertyNames);
                }
                catch (Exception e)
                {
                    Assert.Fail("Assert failed for items in index {0}. Assert Error: {1}", i, e.Message);
                }
            }
        }

        /// <summary>
        /// Asserts the model.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="useActualTypeIfPossible">if set to <b>true</b> uses the actual type of the objects if possible.</param>
        /// <param name="skipPropertyNames">The properties to skip. Names without . will be global for all types. Names with Type prefix sync as SomeClass.SomeProperty will be specific to that type.</param>
        public static void AssertModel<TModel>(TModel expected, TModel actual, bool useActualTypeIfPossible = true, params string[] skipPropertyNames)
        {
            var type = typeof(TModel);
            AssertModel(type, expected, actual, useActualTypeIfPossible, skipPropertyNames);
        }

        /// <summary>
        /// Asserts the model.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="useActualTypeIfPossible">if set to <b>true</b> uses the actual type of the objects if possible.</param>
        /// <param name="skipPropertyNames">The skip property names. Names without . will be global for all types. Names with Type prefix sync as SomeClass.SomeProperty will be specific to that type.</param>
        public static void AssertModel(Type modelType, object expected, object actual, bool useActualTypeIfPossible = true, params string[] skipPropertyNames)
        {
            if (IsScalar(modelType))
            {
                Assert.AreEqual(expected, actual, String.Format(CultureInfo.InvariantCulture, "{0} type model did not match", modelType.FullName));
                return;
            }
            if (expected == null && actual == null)
            {
                return;
            }
            CheckNullMismatch(modelType, expected, actual);

            if (useActualTypeIfPossible && expected.GetType() == actual.GetType())
            {
                //The type of the property might be abstract/some base class and might not have all the properties of the actual objects
                //in which case teu would not be asserted for equality. By using the actual type of the objects all properties of the objects will be asserted.
                modelType = expected.GetType();
            }
            if (IsEnumerable(modelType))
            {
                Type itemType = GetGenericParameterType(modelType);
                AssertEnumerables(useActualTypeIfPossible, skipPropertyNames, expected, actual, modelType, itemType);
                return;
            }

            var properties = modelType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (skipPropertyNames != null && properties.Length > 0)
            {
                var propertiesToSkip = GetSkipPropertyNamesForType(modelType, skipPropertyNames);
                properties = properties.Where(p => !propertiesToSkip.Contains(p.Name)).ToArray();
            }
            foreach (var propertyInfo in properties)
            {
                if (IsEnumerable(propertyInfo.PropertyType))
                {
                    try
                    {
                        Type itemType = GetGenericParameterType(propertyInfo);
                        object expectedValue = propertyInfo.GetValue(expected);
                        object actualValue = propertyInfo.GetValue(actual);
                        AssertEnumerables(useActualTypeIfPossible, skipPropertyNames, expectedValue, actualValue, propertyInfo.PropertyType, itemType);
                    }
                    catch (Exception e)
                    {
                        Assert.Fail("Values did not match for {0} in type {1}. Assert error: {2}", propertyInfo.Name, modelType.Name, e.Message);
                    }
                }
                else
                {
                    try
                    {
                        AssertModel(propertyInfo.PropertyType, propertyInfo.GetValue(expected), propertyInfo.GetValue(actual), useActualTypeIfPossible, skipPropertyNames);
                    }
                    catch (Exception e)
                    {
                        Assert.Fail("Values did not match for {0} in type {1}. Assert error: {2}", propertyInfo.Name, modelType.Name, e.Message);
                    }

                }

            }
        }

        private static void AssertEnumerables(bool useActualTypeIfPossible, string[] skipPropertyNames, object expectedValue, object actualValue, Type objectType, Type itemType)
        {
            if (expectedValue == null && actualValue == null)
            {
                return;
            }
            CheckNullMismatch(objectType, expectedValue, actualValue);
            var expectedList = new ArrayList();
            foreach (var item in expectedValue as IEnumerable)
            {
                expectedList.Add(item);
            }
            var actualList = new ArrayList();
            foreach (var item in actualValue as IEnumerable)
            {
                actualList.Add(item);
            }
            AssertModels(itemType, expectedList, actualList, useActualTypeIfPossible, skipPropertyNames);
        }

        private static void CheckNullMismatch(Type modelType, object expected, object actual)
        {
            if (expected != null && actual == null)
            {
                Assert.Fail("Expected {0} was not null but actual was null.", modelType.Name);
            }
            if (expected == null && actual != null)
            {
                Assert.Fail("Expected {0} was null but actual was not null.", modelType.Name);
            }
        }

        private static string[] GetSkipPropertyNamesForType(Type type, string[] allSkipPropertyNames)
        {
            var global = allSkipPropertyNames.Where(s => !s.Contains(".")).ToArray();
            var typeSpecific = allSkipPropertyNames
                .Where(s => s.Contains(".") && s.StartsWith(type.Name, StringComparison.Ordinal))
                .Select(s => s.Replace(type.Name + ".", String.Empty))
                .ToArray();
            return global.Union(typeSpecific).ToArray();
        }

        private static Type GetGenericParameterType(PropertyInfo propertyInfo)
        {
            return GetGenericParameterType(propertyInfo.PropertyType);
        }

        private static Type GetGenericParameterType(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }
            if (type.BaseType == typeof(Array))
            {
                return type.GetElementType();
            }
            return typeof(object);
        }

        private static bool IsEnumerable(Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static bool IsScalar(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) || type.IsEnum || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Returns true if the given models are equal, false
        /// otherwise.
        /// </summary>
        public static bool ModelsAreEqual<TModel>(TModel expected, TModel actual, bool useActualTypeIfPossible = true, params string[] skipPropertyNames)
        {
            try
            {
                AssertModel(expected, actual, useActualTypeIfPossible, skipPropertyNames);
                return true;
            }
            catch (AssertionException)
            {
                return false;
            }
        }

        /// <summary>
        /// Sleeps the until the condition is satisfied.
        /// </summary>
        /// <param name="condition">The condition. A function that should return true when the waiting can end.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="iterationSleepMilliseconds">How many milliseconds the thread sleeps between each condition check.</param>
        public static void SleepUntil(Func<bool> condition, TimeSpan timeout, int iterationSleepMilliseconds = 50)
        {
            var waitLimit = DateTime.UtcNow.Add(timeout);
            while (!condition() && DateTime.UtcNow < waitLimit)
            {
                Thread.Sleep(iterationSleepMilliseconds);
            }
        }

        /// <summary>
        /// Asserts that the values are in strictly ascending order.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="values">The values.</param>
        /// <param name="propertySelector">The property selector.</param>
        public static void AssertStrictlyAscendingOrder<TValue, TProperty>(IList<TValue> values, Func<TValue, TProperty> propertySelector) where TProperty : IComparable
        {
            for (var i = 0; i < values.Count - 1; ++i)
            {
                Assert.Less(propertySelector(values[i]), propertySelector(values[i + 1]), "The values were not in strictly ascending order.");
            }
        }

        /// <summary>
        /// Asserts that the values are in ascending order.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="values">The values.</param>
        /// <param name="propertySelector">The property selector.</param>
        public static void AssertAscendingOrder<TValue, TProperty>(IList<TValue> values, Func<TValue, TProperty> propertySelector) where TProperty : IComparable
        {
            for (var i = 0; i < values.Count - 1; ++i)
            {
                Assert.LessOrEqual(propertySelector(values[i]), propertySelector(values[i + 1]), "The values were not in ascending order.");
            }
        }

        /// <summary>
        /// Asserts that an argument exception is thrown with the specified <paramref name="paramName"/> when the <paramref name="action"/> is invoked.
        /// </summary>
        /// <typeparam name="TArgumentException">The type of the argument exception.</typeparam>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="action">The action.</param>
        public static void AssertArgumentException<TArgumentException>(string paramName, TestDelegate action)
            where TArgumentException : ArgumentException
        {
            Assert.AreEqual(paramName, Assert.Throws<TArgumentException>(action, "The action did not throw the expected exception.").ParamName, "The parameter name in the exception was incorrect.");
        }
    }
}
