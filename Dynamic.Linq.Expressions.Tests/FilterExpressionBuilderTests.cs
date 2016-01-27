using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TestHelper;

namespace Dynamic.Linq.Expressions.Tests
{
    [TestFixture]
    public class FilterExpressionBuilderTests
    {
        private FilterExpressionBuilder<TestSource> _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new FilterExpressionBuilder<TestSource>();
        }

        [Test]
        public void TestEqualsOperatorString()
        {
            const string foo = "foo";
            var source = new[]
                             {
                                 new TestSource {String = "foo", Int = 1}, new TestSource {String = "bar", Int = 2},
                                 new TestSource {String = "baz", Int = 3}, new TestSource {String = "foo", Int = 4},
                                 new TestSource {String = "foo", Int = 5}, new TestSource {String = "zap", Int = 6},
                                 new TestSource {String = "foo", Int = 7}
                             };

            var expectedResultList = source.Where(t => t.String == foo).ToList();

            var filter = new FilterSetting
                             {OperatorName = FilterOperators.EqualsOperator, PropertyPath = "String", Value = foo};

            var result = _builder.Filter(source.AsQueryable(), new[] {filter});

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestNotEqualsOperatorString()
        {
            const string foo = "foo";
            var source = new[]
                             {
                                 new TestSource {String = "foo", Int = 1}, new TestSource {String = "bar", Int = 2},
                                 new TestSource {String = "baz", Int = 3}, new TestSource {String = "foo", Int = 4},
                                 new TestSource {String = "foo", Int = 5}, new TestSource {String = "zap", Int = 6},
                                 new TestSource {String = "foo", Int = 7}
                             };

            var expectedResultList = source.Where(t => t.String != foo).ToList();

            var filter = new FilterSetting { OperatorName = FilterOperators.NotEqualsOperator, PropertyPath = "String", Value = foo };

            var result = _builder.Filter(source.AsQueryable(), new[] { filter });

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestStartsWithOperatorString()
        {
            const string f = "f";
            var source = new[]
                             {
                                 new TestSource {String = "foo", Int = 1}, new TestSource {String = "bar", Int = 2},
                                 new TestSource {String = "baz", Int = 3}, new TestSource {String = "fii", Int = 4},
                                 new TestSource {String = "fuu", Int = 5}, new TestSource {String = "zap", Int = 6},
                                 new TestSource {String = "faa", Int = 7}
                             };

            var expectedResultList = source.Where(t => t.String.StartsWith(f)).ToList();

            var filter = new FilterSetting { OperatorName = FilterOperators.StartsWithOperator, PropertyPath = "String", Value = f };

            var result = _builder.Filter(source.AsQueryable(), new[] { filter });

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestEndsWithOperatorString()
        {
            const string value = "o";
            var source = new[]
                             {
                                 new TestSource {String = "foo", Int = 1}, new TestSource {String = "baro", Int = 2},
                                 new TestSource {String = "baz", Int = 3}, new TestSource {String = "fii", Int = 4},
                                 new TestSource {String = "fuuo", Int = 5}, new TestSource {String = "zapo", Int = 6},
                                 new TestSource {String = "faa", Int = 7}
                             };

            var expectedResultList = source.Where(t => t.String.EndsWith(value)).ToList();

            var filter = new FilterSetting { OperatorName = FilterOperators.EndsWithOperator, PropertyPath = "String", Value = value };

            var result = _builder.Filter(source.AsQueryable(), new[] { filter });

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestInOperatorString()
        {
            const string value = "goosfraba";
            var source = new[]
                             {
                                 new TestSource {String = "oos", Int = 1}, new TestSource {String = "fra", Int = 2},
                                 new TestSource {String = "ba", Int = 3}, new TestSource {String = "fii", Int = 4},
                                 new TestSource {String = "goos", Int = 5}, new TestSource {String = "osfra", Int = 6},
                                 new TestSource {String = "faa", Int = 7}
                             };

            var expectedResultList = source.Where(t => value.Contains(t.String)).ToList();

            var filter = new FilterSetting { OperatorName = FilterOperators.InOperator, PropertyPath = "String", Value = value };

            var result = _builder.Filter(source.AsQueryable(), new[] { filter });

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestInOperatorEnumerale()
        {
            var value = new[] {"foo", "baz"};
            var source = new[]
                             {
                                 new TestSource {String = "foo", Int = 1}, new TestSource {String = "bar", Int = 2},
                                 new TestSource {String = "baz", Int = 3}, new TestSource {String = "fii", Int = 4},
                                 new TestSource {String = "baz", Int = 5}, new TestSource {String = "fuu", Int = 6},
                                 new TestSource {String = "zap", Int = 7}
                             };

            var expectedResultList = source.Where(t => value.Contains(t.String)).ToList();

            var filter = new FilterSetting { OperatorName = FilterOperators.InOperator, PropertyPath = "String", Value = value };

            var result = _builder.Filter(source.AsQueryable(), new[] { filter });

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestOrExpression()
        {
            var source = new[]
                             {
                                 new TestSource {String = "foo", Int = 1}, new TestSource {String = "bar", Int = 2},
                                 new TestSource {String = "baz", Int = 3}, new TestSource {String = "fii", Int = 4},
                                 new TestSource {String = "baz", Int = 5}, new TestSource {String = "fuu", Int = 6},
                                 new TestSource {String = "zap", Int = 7}
                             };

            var expectedResultList = source.Where(t => t.String == "foo" || t.String == "bar" || t.String == "zap").ToList();

            var fooFilter = new FilterSetting { OperatorName = FilterOperators.EqualsOperator, PropertyPath = "String", Value = "foo" };
            var barFilter = new FilterSetting { OperatorName = FilterOperators.EqualsOperator, PropertyPath = "String", Value = "bar" };
            var zapFilter = new FilterSetting { OperatorName = FilterOperators.EqualsOperator, PropertyPath = "String", Value = "zap" };

            var orFilter = new FilterSetting {OrConnectedFilters = new List<FilterSetting> {fooFilter, barFilter, zapFilter}};

            var result = _builder.Filter(source.AsQueryable(), new[] { orFilter });

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestIdioticWayToSpecifyOrFilters()
        {
            var source = new[]
                             {
                                 new TestSource {String = "foo", Int = 1}, new TestSource {String = "bar", Int = 2},
                                 new TestSource {String = "baz", Int = 3}, new TestSource {String = "fii", Int = 4},
                                 new TestSource {String = "baz", Int = 5}, new TestSource {String = "fuu", Int = 6},
                                 new TestSource {String = "zap", Int = 7}
                             };

            var expectedResultList = source.Where(t => t.String == "foo" || t.String == "bar" || t.String == "zap").ToList();

            var fooFilter = new FilterSetting { OperatorName = FilterOperators.EqualsOperator, PropertyPath = "String", Value = "foo" };
            var barFilter = new FilterSetting { OperatorName = FilterOperators.EqualsOperator, PropertyPath = "String", Value = "bar" };
            var zapFilter = new FilterSetting { OperatorName = FilterOperators.EqualsOperator, PropertyPath = "String", Value = "zap" };

            var zapOrContiner = new FilterSetting {OrConnectedFilters = new List<FilterSetting> {zapFilter}};
            var barOrContainer = new FilterSetting
                                     {
                                         OrConnectedFilters =
                                             new List<FilterSetting>
                                                 {
                                                     new FilterSetting
                                                         {OrConnectedFilters = new List<FilterSetting> {barFilter}}
                                                 }
                                     };
            var fooOrContainer = new FilterSetting
                                     {
                                         OrConnectedFilters =
                                             new List<FilterSetting>
                                                 {
                                                     new FilterSetting
                                                         {
                                                             OrConnectedFilters =
                                                                 new List<FilterSetting>
                                                                     {
                                                                         new FilterSetting
                                                                             {
                                                                                 OrConnectedFilters =
                                                                                     new List<FilterSetting> {fooFilter}
                                                                             }
                                                                     }
                                                         }
                                                 }
                                     };

            var orFilter = new FilterSetting { OrConnectedFilters = new List<FilterSetting> { fooOrContainer, barOrContainer, zapOrContiner } };

            var result = _builder.Filter(source.AsQueryable(), new[] { orFilter });

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestOrAndCombination()
        {
            var source = new[]
                             {
                                 new TestSource {String = "foo", Int = 1}, new TestSource {String = "bar", Int = 2},
                                 new TestSource {String = "baz", Int = 3}, new TestSource {String = "fii", Int = 4},
                                 new TestSource {String = "baz", Int = 5}, new TestSource {String = "fuu", Int = 6},
                                 new TestSource {String = "zap", Int = 7}, new TestSource {String = "faa", Int = 8}
                             };

            var expectedResultList = source.Where(t => t.String.StartsWith("f") && (t.String.EndsWith("i") || t.String.EndsWith("a"))).ToList();

            var startsWithFilter = new FilterSetting { OperatorName = FilterOperators.StartsWithOperator, PropertyPath = "String", Value = "f" };
            var endsWith1Filter = new FilterSetting { OperatorName = FilterOperators.EndsWithOperator, PropertyPath = "String", Value = "i" };
            var endsWith2Filter = new FilterSetting { OperatorName = FilterOperators.EndsWithOperator, PropertyPath = "String", Value = "a" };

            var orFilter = new FilterSetting { OrConnectedFilters = new List<FilterSetting> { endsWith1Filter, endsWith2Filter } };

            var result = _builder.Filter(source.AsQueryable(), new[] { startsWithFilter, orFilter });

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestPropertyPathFiltering()
        {
            const string foo = "foo";
            var source = new[]
                             {
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 1}}}, 
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "bar", Int = 2}}},
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "baz", Int = 3}}}, 
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 4}}},
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 5}}}, 
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "zap", Int = 6}}},
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 7}}}
                             };

            var expectedResultList = source.Where(t => t.Inner.Complex.String == foo).ToList();

            var filter = new FilterSetting { OperatorName = FilterOperators.EqualsOperator, PropertyPath = "Inner.Complex.String", Value = foo };

            var result = _builder.Filter(source.AsQueryable(), new[] { filter });

            TestUtil.AssertModels(expectedResultList, result.ToList());
        }

        [Test]
        public void TestPropertyPathFilteringWithNullCheck()
        {
            const string foo = "foo";
            var source = new[]
                             {
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 1}}}, 
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = null, Int = 2}}},//Final property is null
                                 new TestSource{ Inner = new InnerType{Complex = null}}, //Null in between
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 4}}},
                                 new TestSource{ Inner = null}, //Null in between
                                 null,//Whole item is null
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 7}}}
                             };

            var expectedResultList = source.AsQueryable().Where(t => t != null && t.Inner != null && t.Inner.Complex != null && t.Inner.Complex.String == foo);

            var filter = new FilterSetting { OperatorName = FilterOperators.EqualsOperator, PropertyPath = "Inner.Complex.String", Value = foo };

            var result = _builder.Filter(source.AsQueryable(), new[] { filter });

            TestUtil.AssertModels(expectedResultList.ToList(), result.ToList());
        }
    }
}
