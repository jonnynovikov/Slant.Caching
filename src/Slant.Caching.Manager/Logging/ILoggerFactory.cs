namespace Slant.Caching.Manager.Logging;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600
public interface ILoggerFactory
{
    ILogger CreateLogger(string categoryName);

    ILogger CreateLogger<T>(T instance);
}

public class NullLoggerFactory : ILoggerFactory
{
    public ILogger CreateLogger(string categoryName)
    {
        return new NullLogger();
    }

    public ILogger CreateLogger<T>(T instance)
    {
        return new NullLogger();
    }
}
#pragma warning restore SA1600
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member