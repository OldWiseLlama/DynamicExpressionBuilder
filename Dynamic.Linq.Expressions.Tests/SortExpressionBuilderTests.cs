using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TestHelper;

namespace Dynamic.Linq.Expressions.Tests
{
    [TestFixture]
    public class SortExpressionBuilderTests
    {
        private SortExpressionBuilder<TestSource> _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new SortExpressionBuilder<TestSource>();
        }

        [Test]
        public void SortByImmediateProperties()
        {
            var source = new[]
                             {
                                 new TestSource {String = "foo", Int = 1}, new TestSource {String = "bar", Int = 2},
                                 new TestSource {String = "baz", Int = 2}, new TestSource {String = "foo", Int = 8},
                                 new TestSource {String = "foo", Int = 99}, new TestSource {String = "zap", Int = 6},
                                 new TestSource {String = "foo", Int = 7}
                             };

            var sortSettings = new List<SortSetting>
                                   {
                                       new SortSetting {PropertyPath = "String", SortOrder = SortOrder.Ascending},
                                       new SortSetting {PropertyPath = "Int", SortOrder = SortOrder.Descending}
                                   };

            IQueryable<TestSource> queryable = source.AsQueryable();

            IOrderedQueryable<TestSource> expectedSortedQueryable = queryable.OrderBy(s => s.String).ThenByDescending(s => s.Int);

            var result = _builder.Sort(queryable, sortSettings);

            TestUtil.AssertModels(expectedSortedQueryable.ToList(), result.ToList());
        }

        [Test]
        public void SortByPorpertyPath()
        {
            var source = new[]
                             {
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 1}}}, 
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "bar", Int = 99}}},
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "baz", Int = 3}}}, 
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 8}}},
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 5}}}, 
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "zap", Int = 16}}},
                                 new TestSource{ Inner = new InnerType{Complex = new TestSource {String = "foo", Int = 7}}}
                             };

            var sortSettings = new List<SortSetting>
                                   {
                                       new SortSetting {PropertyPath = "Inner.Complex.String", SortOrder = SortOrder.Ascending},
                                       new SortSetting {PropertyPath = "Inner.Complex.Int", SortOrder = SortOrder.Descending}
                                   };

            IQueryable<TestSource> queryable = source.AsQueryable();

            IOrderedQueryable<TestSource> expectedSortedQueryable = queryable.OrderBy(s => s.Inner.Complex.String).ThenByDescending(s => s.Inner.Complex.Int);

            var result = _builder.Sort(queryable, sortSettings);

            TestUtil.AssertModels(expectedSortedQueryable.ToList(), result.ToList());
        }
    }
}
