namespace EPR.Common.Functions.Test.Database;

using System.Linq.Expressions;
using EPR.Common.Functions.Database;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DelegateReflectionTests
{
    [TestMethod]
    public void ConstructorOf_ReturnsConstructorInfo()
    {
        var ctor = DelegateReflection.ConstructorOf<Func<TestClass>>(() => new TestClass("test"));

        ctor.Should().NotBeNull();
        ctor.DeclaringType.Should().Be(typeof(TestClass));
    }

    [TestMethod]
    public void MethodOf_FromMethodCall_ReturnsMethodInfo()
    {
        var method = DelegateReflection.MethodOf<Func<string>>(() => "test".ToUpper());

        method.Should().NotBeNull();
        method.Name.Should().Be("ToUpper");
    }

    [TestMethod]
    public void MethodOf_ThrowsNotSupportedException_ForUnsupportedExpression()
    {
        Expression<Func<int>> expr = () => 42;
        var act = () => DelegateReflection.MethodOf(expr);

        act.Should().Throw<NotSupportedException>();
    }

    [TestMethod]
    public void GenericMethodOf_ReturnsGenericMethodDefinition()
    {
        var method = DelegateReflection.GenericMethodOf<Func<IEnumerable<string>>>(() => Enumerable.Empty<string>());

        method.Should().NotBeNull();
        method.IsGenericMethodDefinition.Should().BeTrue();
    }

    [TestMethod]
    public void PropertyOf_ReturnsPropertyInfo()
    {
        var prop = DelegateReflection.PropertyOf<Func<int>>(() => string.Empty.Length);

        prop.Should().NotBeNull();
        prop.Name.Should().Be("Length");
    }

    public class TestClass
    {
        public TestClass(string value)
        {
            this.Value = value;
        }

        public string Value { get; }
    }
}
