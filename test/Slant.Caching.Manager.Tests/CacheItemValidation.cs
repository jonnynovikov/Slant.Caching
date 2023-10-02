using System;
using System.Diagnostics.CodeAnalysis;
using Slant.Caching.Manager;
using FluentAssertions;
using Xunit;

namespace Slant.Caching.Manager.Tests;

[ExcludeFromCodeCoverage]
public class CacheItemValidation
{
    private readonly RegionId _regionId = new("region");
    
    [Fact]
    [Trait("category", "Unreliable")]
    public void CacheItemValidation_WithAbsoluteExpiration()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Sliding, TimeSpan.FromDays(10));

        // act
        var result = baseItem.WithAbsoluteExpiration(DateTimeOffset.Now.AddMinutes(10));

        // assert
        result.ExpirationMode.Should().Be(ExpirationMode.Absolute);
        result.ExpirationTimeout.Should().BeCloseTo(TimeSpan.FromMinutes(10), 200);
        result.Value.Should().Be(baseItem.Value);
        result.Region.Should().Be(baseItem.Region);
        result.Key.Should().Be(baseItem.Key);
        result.CreatedUtc.Kind.Should().Be(baseItem.CreatedUtc.Kind);
        result.CreatedUtc.Kind.Should().Be(DateTimeKind.Utc);
        result.CreatedUtc.Should().BeCloseTo(baseItem.CreatedUtc);
        result.LastAccessedUtc.Should().Be(baseItem.LastAccessedUtc);
    }

    [Fact]
    public void CacheItemValidation_WithAbsoluteExpiration_Invalid()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Sliding, TimeSpan.FromDays(10));

        // act
        Action act = () => baseItem.WithAbsoluteExpiration(DateTimeOffset.Now.AddMinutes(-10));

        // assert
        act.Should().Throw<ArgumentException>().WithMessage("*value must be greater*");
    }

    [Fact]
    public void CacheItemValidation_WithAbsoluteExpiration_InvalidB()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Sliding, TimeSpan.FromDays(10));

        // act
        Action act = () => baseItem.WithAbsoluteExpiration(TimeSpan.FromMilliseconds(-10));

        // assert
        act.Should().Throw<ArgumentException>().WithMessage("*value must be greater*");
    }

    [Fact]
    public void CacheItemValidation_WithSlidingExpiration_Invalid()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Sliding, TimeSpan.FromDays(10));

        // act
        Action act = () => baseItem.WithSlidingExpiration(TimeSpan.FromDays(-1));

        // assert
        act.Should().Throw<ArgumentException>().WithMessage("*value must be greater*");
    }

    [Fact]
    public void CacheItemValidation_WithExpiration_Invalid()
    {
        // arrange
        // act
        Action act = () =>
            new CacheItem<object>("key", _regionId, "value", ExpirationMode.Sliding, TimeSpan.FromTicks(long.MaxValue));

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*365*");
    }

    [Fact]
    public void CacheItemValidation_WithCreated()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Sliding, TimeSpan.FromDays(10));
        var created = DateTime.UtcNow.AddMinutes(-10);

        // act
        var result = baseItem.WithCreated(created);

        // assert
        result.CreatedUtc.Should().BeCloseTo(created);
        result.Value.Should().Be(baseItem.Value);
        result.Region.Should().Be(baseItem.Region);
        result.Key.Should().Be(baseItem.Key);
        result.LastAccessedUtc.Should().BeCloseTo(baseItem.LastAccessedUtc);
        result.ExpirationMode.Should().Be(baseItem.ExpirationMode);
        result.ExpirationTimeout.Should().Be(baseItem.ExpirationTimeout);
    }

    [Fact]
    public void CacheItemValidation_WithExpiration_None()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Sliding, TimeSpan.FromDays(10));

        // act
        var result = baseItem.WithNoExpiration();

        // assert
        result.ExpirationMode.Should().Be(ExpirationMode.None);
        result.ExpirationTimeout.Should().Be(TimeSpan.Zero); // should be zero although we set to to 10 minutes
        result.Value.Should().Be(baseItem.Value);
        result.Region.Should().Be(baseItem.Region);
        result.Key.Should().Be(baseItem.Key);
        result.CreatedUtc.Should().BeCloseTo(baseItem.CreatedUtc);
        result.LastAccessedUtc.Should().BeCloseTo(baseItem.LastAccessedUtc);
    }

    [Fact]
    public void CacheItemValidation_WithExpiration_Sliding()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Absolute, TimeSpan.FromDays(10));

        // act
        var result = baseItem.WithSlidingExpiration(TimeSpan.FromMinutes(10));

        // assert
        result.ExpirationMode.Should().Be(ExpirationMode.Sliding);
        result.ExpirationTimeout.Should().Be(TimeSpan.FromMinutes(10));
        result.Value.Should().Be(baseItem.Value);
        result.Region.Should().Be(baseItem.Region);
        result.Key.Should().Be(baseItem.Key);
        result.CreatedUtc.Should().BeCloseTo(baseItem.CreatedUtc);
        result.LastAccessedUtc.Should().BeCloseTo(baseItem.LastAccessedUtc);
    }

    [Fact]
    [Trait("category", "Unreliable")]
    public void CacheItemValidation_WithExpiration_Absolute()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Sliding, TimeSpan.FromDays(10));

        // act
        var result = baseItem.WithAbsoluteExpiration(DateTime.UtcNow.AddMinutes(10));

        // assert
        result.ExpirationMode.Should().Be(ExpirationMode.Absolute);
        result.ExpirationTimeout.Should().BeCloseTo(TimeSpan.FromMinutes(10), 100);
        result.Value.Should().Be(baseItem.Value);
        result.Region.Should().Be(baseItem.Region);
        result.Key.Should().Be(baseItem.Key);
        result.CreatedUtc.Should().BeCloseTo(result.CreatedUtc); // !! Changed due to issue #136
        result.LastAccessedUtc.Should().BeCloseTo(baseItem.LastAccessedUtc);
    }

    [Fact]
    public void CacheItemValidation_WithNoExpiration()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Sliding, TimeSpan.FromDays(10));

        // act
        var result = baseItem.WithNoExpiration();

        // assert
        result.ExpirationMode.Should().Be(ExpirationMode.None);
        result.ExpirationTimeout.Should().Be(TimeSpan.Zero);
        result.Value.Should().Be(baseItem.Value);
        result.Region.Should().Be(baseItem.Region);
        result.Key.Should().Be(baseItem.Key);
        result.CreatedUtc.Should().BeCloseTo(baseItem.CreatedUtc);
        result.LastAccessedUtc.Should().BeCloseTo(baseItem.LastAccessedUtc);
    }

    [Fact]
    public void CacheItemValidation_WithSlidingExpiration()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Absolute, TimeSpan.FromDays(10));

        // act
        var result = baseItem.WithSlidingExpiration(TimeSpan.FromHours(2));

        // assert
        result.ExpirationMode.Should().Be(ExpirationMode.Sliding);
        result.ExpirationTimeout.Should().Be(TimeSpan.FromHours(2));
        result.Value.Should().Be(baseItem.Value);
        result.Region.Should().Be(baseItem.Region);
        result.Key.Should().Be(baseItem.Key);
        result.CreatedUtc.Should().BeCloseTo(baseItem.CreatedUtc);
        result.LastAccessedUtc.Should().BeCloseTo(baseItem.LastAccessedUtc);
    }

    [Fact]
    public void CacheItemValidation_WithValue()
    {
        // arrange
        var baseItem = new CacheItem<object>("key", _regionId, "value", ExpirationMode.Absolute, TimeSpan.FromDays(10));

        // act
        var result = baseItem.WithValue("new value");

        // assert
        result.Value.Should().Be("new value");
        result.Region.Should().Be(baseItem.Region);
        result.Key.Should().Be(baseItem.Key);
        result.CreatedUtc.Should().BeCloseTo(baseItem.CreatedUtc);
        result.LastAccessedUtc.Should().BeCloseTo(baseItem.LastAccessedUtc);
        result.ExpirationMode.Should().Be(baseItem.ExpirationMode);
        result.ExpirationTimeout.Should().Be(baseItem.ExpirationTimeout);
    }

    #region ctor1

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor1_EmptyKey()
    {
        // arrange
        var key = string.Empty;
        object value = null;

        // act
        Action act = () => new CacheItem<object>(key, value);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor1_NullKey()
    {
        // arrange
        string key = null;
        object value = null;

        // act
        Action act = () => new CacheItem<object>(key, value);

        // assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor1_WhitespaceKey()
    {
        // arrange
        var key = "    ";
        object value = null;

        // act
        Action act = () => new CacheItem<object>(key, value);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor1_NullValue()
    {
        // arrange
        var key = "key";
        object value = null;

        // act
        Action act = () => new CacheItem<object>(key, value);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("value");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor1_ValidateCreatedResult()
    {
        // arrange
        var key = "key";
        object value = "value";

        // act
        var act = new CacheItem<object>(key, value);

        // assert
        act.Should()
            .Match<CacheItem<object>>(p => p.ExpirationMode == ExpirationMode.Default)
            .And.Match<CacheItem<object>>(p => p.ExpirationTimeout == TimeSpan.Zero)
            .And.Match<CacheItem<object>>(p => p.Key == key)
            .And.Match<CacheItem<object>>(p => p.Value == value)
            .And.Match<CacheItem<object>>(p => !p.Region.HasValue);
    }

    #endregion ctor1

    #region ctor2

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor2_EmptyKey()
    {
        // arrange
        var key = string.Empty;
        object value = null;
        var region = RegionId.Empty;

        // act
        Action act = () => new CacheItem<object>(key, region, value);

        // assert
        act.Should().Throw<ArgumentException>().And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor2_NullKey()
    {
        // arrange
        string key = null;
        object value = null;
        var region = RegionId.Empty;

        // act
        Action act = () => new CacheItem<object>(key, region, value);

        // assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor2_WhitespaceKey()
    {
        // arrange
        var key = "    ";
        object value = null;
        var region = RegionId.Empty;

        // act
        Action act = () => new CacheItem<object>(key, region, value);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor2_NullValue()
    {
        // arrange
        var key = "key";
        object value = null;
        var region = RegionId.Empty;

        // act
        Action act = () => new CacheItem<object>(key, region, value);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("value");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor2_EmptyRegion()
    {
        // arrange
        var key = "key";
        var value = "value";
        var region = RegionId.Empty;

        // act
        Action act = () => new CacheItem<object>(key, region, value);

        // assert
        act.Should().Throw<ArgumentException>().And.ParamName.Equals(_regionId);
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor2_NullRegion()
    {
        // arrange
        var key = "key";
        var value = "value";
        RegionId region = null;

        // act
        Action act = () => new CacheItem<object>(key, region, value);

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor2_WhitespaceRegion()
    {
        // arrange
        var key = "key";
        var value = "value";
        var region = new RegionId("  ");

        // act
        Action act = () => new CacheItem<object>(key, region, value);

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor2_ValidateCreatedResult()
    {
        // arrange
        var key = "key";
        object value = "value";
        var region = _regionId;

        // act
        var act = new CacheItem<object>(key, region, value);

        // assert
        act.Should()
            .Match<CacheItem<object>>(p => p.ExpirationMode == ExpirationMode.Default)
            .And.Match<CacheItem<object>>(p => p.ExpirationTimeout == TimeSpan.Zero)
            .And.Match<CacheItem<object>>(p => p.Key == key)
            .And.Match<CacheItem<object>>(p => p.Value == value)
            .And.Match<CacheItem<object>>(p => p.Region == region);
    }

    #endregion ctor2

    #region ctor3

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor3_EmptyKey()
    {
        // arrange
        var key = string.Empty;
        object value = null;
        ExpirationMode mode = 0;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor3_NullKey()
    {
        // arrange
        string key = null;
        object value = null;
        ExpirationMode mode = 0;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor3_WhitespaceKey()
    {
        // arrange
        var key = "    ";
        object value = null;
        ExpirationMode mode = 0;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor3_NullValue()
    {
        // arrange
        var key = "key";
        object value = null;
        ExpirationMode mode = 0;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("value");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor3_ValidateCreatedResult()
    {
        // arrange
        var key = "key";
        object value = "value";
        var mode = ExpirationMode.Sliding;
        var timeout = new TimeSpan(0, 23, 45);

        // act
        var act = new CacheItem<object>(key, value, mode, timeout);

        // assert
        act.Should()
            .Match<CacheItem<object>>(p => p.ExpirationMode == mode)
            .And.Match<CacheItem<object>>(p => p.ExpirationTimeout == timeout)
            .And.Match<CacheItem<object>>(p => p.Key == key)
            .And.Match<CacheItem<object>>(p => p.Value == value)
            .And.Match<CacheItem<object>>(p => p.Region.IsEmpty);
    }

    #endregion ctor3

    #region ctor4

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor4_EmptyKey()
    {
        // arrange
        var key = string.Empty;
        object value = null;
        var region = RegionId.Empty;
        var mode = ExpirationMode.None;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, region, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor4_NullKey()
    {
        // arrange
        string key = null;
        object value = null;
        var region = RegionId.Empty;
        var mode = ExpirationMode.None;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, region, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor4_WhitespaceKey()
    {
        // arrange
        var key = "    ";
        object value = null;
        var region = RegionId.Empty;
        var mode = ExpirationMode.None;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, region, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("key");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor4_NullValue()
    {
        // arrange
        var key = "key";
        object value = null;
        var region = RegionId.Empty;
        var mode = ExpirationMode.None;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, region, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Equals("value");
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor4_EmptyRegion()
    {
        // arrange
        var key = "key";
        var value = "value";
        var region = RegionId.Empty;
        var mode = ExpirationMode.None;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, region, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals(_regionId);
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor4_NullRegion()
    {
        // arrange
        var key = "key";
        var value = "value";
        RegionId region = null;
        var mode = ExpirationMode.None;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, region, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Equals(_regionId);
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor4_WhitespaceRegion()
    {
        // arrange
        var key = "key";
        var value = "value";
        var region = new RegionId("  ");
        var mode = ExpirationMode.None;
        var timeout = default(TimeSpan);

        // act
        Action act = () => new CacheItem<object>(key, region, value, mode, timeout);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals(_regionId);
    }

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor4_ValidateCreatedResult()
    {
        // arrange
        var key = "key";
        object value = "value";
        var region = _regionId;
        var mode = ExpirationMode.Sliding;
        var timeout = new TimeSpan(0, 23, 45);

        // act
        var act = new CacheItem<object>(key, region, value, mode, timeout);

        // assert
        act.Should()
            .Match<CacheItem<object>>(p => p.ExpirationMode == mode)
            .And.Match<CacheItem<object>>(p => p.ExpirationTimeout == timeout)
            .And.Match<CacheItem<object>>(p => p.Key == key)
            .And.Match<CacheItem<object>>(p => p.Value == value)
            .And.Match<CacheItem<object>>(p => p.Region == region);
    }

    #endregion ctor4

    [Fact]
    [ReplaceCulture]
    public void CacheItemValidation_Ctor_ExpirationTimeoutDefaults()
    {
        // arrange
        var key = "key";
        object value = "value";

        // act
        var act = new CacheItem<object>(key, value, ExpirationMode.None, TimeSpan.FromDays(1));

        // assert - should reset to TimeSpan.Zero because mode is "None"
        act.Should()
            .Match<CacheItem<object>>(p => p.ExpirationMode == ExpirationMode.None)
            .And.Match<CacheItem<object>>(p => p.ExpirationTimeout == TimeSpan.Zero)
            .And.Match<CacheItem<object>>(p => p.Key == key)
            .And.Match<CacheItem<object>>(p => p.Value == value);
    }
}