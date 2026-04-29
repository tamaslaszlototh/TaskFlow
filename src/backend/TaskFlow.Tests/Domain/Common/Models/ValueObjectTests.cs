using FluentAssertions;
using TaskFlow.Domain.Common.Models;

namespace TaskFlow.Tests.Domain.Common.Models;

public class ValueObjectTests
{
    [Fact]
    public void Equals_SameComponents_ReturnsTrue()
    {
        var obj1 = new TestValueObject("value1", "value2");
        var obj2 = new TestValueObject("value1", "value2");

        var result = obj1.Equals(obj2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentComponents_ReturnsFalse()
    {
        var obj1 = new TestValueObject("value1", "value2");
        var obj2 = new TestValueObject("different", "value2");

        var result = obj1.Equals(obj2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var obj = new TestValueObject("value1", "value2");

        var result = obj.Equals(null);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var obj = new TestValueObject("value1", "value2");

        var result = obj.Equals("not a value object");

        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameComponents_ReturnsSameHashCode()
    {
        var obj1 = new TestValueObject("value1", "value2");
        var obj2 = new TestValueObject("value1", "value2");

        obj1.GetHashCode().Should().Be(obj2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentComponents_ReturnsDifferentHashCode()
    {
        var obj1 = new TestValueObject("value1", "value2");
        var obj2 = new TestValueObject("different", "value2");

        obj1.GetHashCode().Should().NotBe(obj2.GetHashCode());
    }

    private class TestValueObject(string prop1, string prop2) : ValueObject
    {
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return prop1;
            yield return prop2;
        }
    }
}
