using System;
using NUnit.Framework;

namespace Dynamic.Linq.Expressions.Tests
{
    [TestFixture]
    public class ExpressionHelperUnitTests
    {
        [Test]
        public void TestImmediateProperties()
        {
            var type = typeof(TestSource);
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("String", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Int", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Decimal", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("DateTime", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Inner", type));
            Assert.False(ExpressionHelper.IsPropertyPathValid(null, type));
            Assert.False(ExpressionHelper.IsPropertyPathValid(String.Empty, type));
            Assert.False(ExpressionHelper.IsPropertyPathValid("  ", type));
            Assert.False(ExpressionHelper.IsPropertyPathValid("Foo", type));
        }

        [Test]
        public void TestPropertyPaths()
        {
            var type = typeof(TestSource);
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Inner.String", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Inner.Int", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Inner.Decimal", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Inner.DateTime", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Inner.Complex", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Inner.Complex.Int", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Inner.Complex.Inner.String", type));
            Assert.IsTrue(ExpressionHelper.IsPropertyPathValid("Inner.Complex.Inner.Complex.DateTime", type));
            Assert.False(ExpressionHelper.IsPropertyPathValid("Inner.", type));
            Assert.False(ExpressionHelper.IsPropertyPathValid("Inner. ", type));
            Assert.False(ExpressionHelper.IsPropertyPathValid("Inner.Foo", type));

        }
    }
}
