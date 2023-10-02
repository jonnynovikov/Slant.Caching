using System;

namespace Slant.Caching.Manager;

public sealed record RegionId(string Value)
{
    public static RegionId Empty => new(string.Empty);

    public bool HasValue => !string.IsNullOrEmpty(Value);

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public bool Equals(RegionId? other)
    {
        return Value?.Equals(other?.Value, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
};

public sealed record KeyRegion(string Key, RegionId Region);