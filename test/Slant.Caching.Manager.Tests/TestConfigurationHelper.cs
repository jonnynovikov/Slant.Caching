using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Slant.Caching.Manager.Utility.Guard;

namespace Slant.Caching.Manager.Tests;

[ExcludeFromCodeCoverage]
public static class TestConfigurationHelper
{
    public static string GetCfgFileName(string fileName)
    {
        NotNullOrWhiteSpace(fileName, nameof(fileName));
        var basePath = Environment.CurrentDirectory;
        return basePath + (fileName.StartsWith("/") ? fileName : "/" + fileName);
    }
}