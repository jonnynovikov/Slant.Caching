using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Xunit.Sdk;

namespace Slant.Caching.Manager.Tests;

[ExcludeFromCodeCoverage]
[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "nope")]
[AttributeUsage(AttributeTargets.Method)]
public sealed class ReplaceCultureAttribute : BeforeAfterTestAttribute
{
    private const string DefaultCultureName = "en-GB";
    private const string DefaultUICultureName = "en-US";
    private CultureInfo originalCulture;
    private CultureInfo originalUICulture;

    public ReplaceCultureAttribute()
        : this(DefaultCultureName, DefaultUICultureName)
    {
    }

    public ReplaceCultureAttribute(string currentCulture, string currentUICulture)
    {
        CurrentCulture = new CultureInfo(currentCulture);
        CurrentUICulture = new CultureInfo(currentUICulture);
    }

    public CultureInfo CurrentCulture { get; }

    public CultureInfo CurrentUICulture { get; }

    public override void Before(MethodInfo methodUnderTest)
    {
        originalCulture = CultureInfo.CurrentCulture;
        originalUICulture = CultureInfo.CurrentUICulture;

        Thread.CurrentThread.CurrentCulture = CurrentCulture;
        Thread.CurrentThread.CurrentUICulture = CurrentUICulture;
    }

    public override void After(MethodInfo methodUnderTest)
    {
        Thread.CurrentThread.CurrentCulture = originalCulture;
        Thread.CurrentThread.CurrentUICulture = originalUICulture;
    }
}