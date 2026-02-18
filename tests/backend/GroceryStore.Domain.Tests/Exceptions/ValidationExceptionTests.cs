using FluentAssertions;
using GroceryStore.Domain.Exceptions;

namespace GroceryStore.Domain.Tests.Exceptions;

public class ValidationExceptionTests
{
    // ───────────────── ThrowIfNullOrWhiteSpace ─────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ThrowIfNullOrWhiteSpace_NullOrWhitespace_Throws(string? value)
    {
        var act = () => ValidationException.ThrowIfNullOrWhiteSpace(value);

        act.Should().Throw<ValidationException>()
           .WithMessage("*is required*");
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_ValidString_DoesNotThrow()
    {
        var act = () => ValidationException.ThrowIfNullOrWhiteSpace("hello");

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfNullOrWhiteSpace_CapturesParameterName()
    {
        string? myParam = null;

        var act = () => ValidationException.ThrowIfNullOrWhiteSpace(myParam);

        act.Should().Throw<ValidationException>()
           .WithMessage("*myParam*");
    }

    // ───────────────── ThrowIfTooLong ─────────────────

    [Fact]
    public void ThrowIfTooLong_ExceedsMax_Throws()
    {
        var longString = new string('a', 11);

        var act = () => ValidationException.ThrowIfTooLong(longString, maxLen: 10);

        act.Should().Throw<ValidationException>()
           .WithMessage("*at most 10*");
    }

    [Fact]
    public void ThrowIfTooLong_ExactLength_DoesNotThrow()
    {
        var exact = new string('a', 10);

        var act = () => ValidationException.ThrowIfTooLong(exact, maxLen: 10);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ThrowIfTooLong_NullOrWhitespace_DoesNotThrow(string? value)
    {
        var act = () => ValidationException.ThrowIfTooLong(value, maxLen: 5);

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfTooLong_TrimsBeforeChecking()
    {
        // "  abc  " trims to "abc" (3 chars) — should NOT exceed maxLen 3
        var act = () => ValidationException.ThrowIfTooLong("  abc  ", maxLen: 3);

        act.Should().NotThrow();
    }

    // ───────────────── ThrowIfOutOfRange ─────────────────

    [Theory]
    [InlineData(-1, 0, 100)]
    [InlineData(101, 0, 100)]
    public void ThrowIfOutOfRange_OutOfBounds_Throws(int value, int min, int max)
    {
        var act = () => ValidationException.ThrowIfOutOfRange(value, min, max);

        act.Should().Throw<ValidationException>()
           .WithMessage($"*between {min} and {max}*");
    }

    [Theory]
    [InlineData(0, 0, 100)]
    [InlineData(50, 0, 100)]
    [InlineData(100, 0, 100)]
    public void ThrowIfOutOfRange_WithinBounds_DoesNotThrow(int value, int min, int max)
    {
        var act = () => ValidationException.ThrowIfOutOfRange(value, min, max);

        act.Should().NotThrow();
    }

    // ───────────────── ThrowIfNegative ─────────────────

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void ThrowIfNegative_NegativeValue_Throws(decimal value)
    {
        var act = () => ValidationException.ThrowIfNegative(value);

        act.Should().Throw<ValidationException>()
           .WithMessage("*non-negative*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999.99)]
    public void ThrowIfNegative_NonNegativeValue_DoesNotThrow(decimal value)
    {
        var act = () => ValidationException.ThrowIfNegative(value);

        act.Should().NotThrow();
    }

    // ───────────────── ThrowIfNull ─────────────────

    [Fact]
    public void ThrowIfNull_NullObject_Throws()
    {
        object? obj = null;

        var act = () => ValidationException.ThrowIfNull(obj);

        act.Should().Throw<ValidationException>()
           .WithMessage("*is required*");
    }

    [Fact]
    public void ThrowIfNull_NonNullObject_DoesNotThrow()
    {
        var obj = new object();

        var act = () => ValidationException.ThrowIfNull(obj);

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfNull_CapturesParameterName()
    {
        object? myObject = null;

        var act = () => ValidationException.ThrowIfNull(myObject);

        act.Should().Throw<ValidationException>()
           .WithMessage("*myObject*");
    }

    // ───────────────── DomainException base ─────────────────

    [Fact]
    public void ValidationException_IsDomainException()
    {
        var ex = new ValidationException("test");

        ex.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void DomainException_CarriesMessage()
    {
        var ex = new DomainException("something went wrong");

        ex.Message.Should().Be("something went wrong");
    }
}
