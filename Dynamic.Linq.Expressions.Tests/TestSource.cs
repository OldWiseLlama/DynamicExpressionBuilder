using System;
using System.Diagnostics;

namespace Dynamic.Linq.Expressions.Tests
{
    [DebuggerDisplay("String: {String}, Int: {Int}, Decimal: {Decimal}, DateTime: {DateTime}, InnerObject: {InnerObject}")]
    public class TestSource
    {
        public string String { get; set; }

        public int Int { get; set; }

        public decimal Decimal { get; set; }

        public DateTime DateTime { get; set; }

        public InnerType Inner { get; set; }
    }

    public class InnerType
    {
        public string String { get; set; }

        public int Int { get; set; }

        public decimal Decimal { get; set; }

        public DateTime DateTime { get; set; }

        public TestSource Complex { get; set; }
    }
}
