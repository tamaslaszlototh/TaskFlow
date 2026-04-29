using FluentAssertions;
using TaskFlow.Domain.Common.Models;

namespace TaskFlow.Tests.Domain.Common.Models;

public class EntityTests
{
    [Fact]
    public void Constructor_ThrowsArgumentException_WhenIdIsEmpty()
    {
        var act = () => new TestEntity(Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Id cannot be empty*");
    }

    [Fact]
    public void Constructor_AcceptsValidId()
    {
        var id = Guid.NewGuid();

        var entity = new TestEntity(id);

        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        var result = entity1.Equals(entity2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        var result = entity1.Equals(entity2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());

        var result = entity.Equals(null);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());

        var result = entity.Equals("not an entity");

        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ReturnsIdHashCode()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        entity.GetHashCode().Should().Be(id.GetHashCode());
    }

    private class TestEntity(Guid id) : Entity(id);
}
