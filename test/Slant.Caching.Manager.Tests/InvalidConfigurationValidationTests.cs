using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Slant.Caching.Manager;
using Slant.Caching.Manager.Configuration;
using Slant.Caching.Manager.Internal;
using FluentAssertions;
using Xunit;

namespace Slant.Caching.Manager.Tests;

/// <summary>
/// To run the test, the app.config of the test project must at least contain a cacheManager section.
/// </summary>
[ExcludeFromCodeCoverage]
public class InvalidConfigurationValidationTests
{
    [Fact]
    [ReplaceCulture]
    public void Cfg_BuildConfiguration_MissingSettings()
    {
        // arrange act
        Action act = () => ConfigurationBuilder.BuildConfiguration(null);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("settings");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_LoadConfiguration_EmptyString()
    {
        // arrange
        var cfgName = string.Empty;

        // act
        Action act = () => ConfigurationBuilder.LoadConfiguration(cfgName);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("configName");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_LoadConfiguration_NullString()
    {
        // arrange
        string cfgName = null;

        // act
        Action act = () => ConfigurationBuilder.LoadConfiguration(cfgName);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("configName");
    }

#if !NO_APP_CONFIG

    [Fact]
    [ReplaceCulture]
    [Trait("category", "NotOnMono")]
    public void Cfg_LoadConfiguration_NotExistingCacheCfgName()
    {
        // arrange
        var cfgName = Guid.NewGuid().ToString();

        // act
        Action act = () => ConfigurationBuilder.LoadConfiguration(cfgName);

        // assert
        act.Should().Throw<InvalidOperationException>().WithMessage("No section defined with name*");
    }

#endif

    [Fact]
    [ReplaceCulture]
    public void Cfg_LoadConfiguration_InvalidSectionName()
    {
        // arrange act
        Action act = () => ConfigurationBuilder.LoadConfiguration(null, "config");

        // assert
        act.Should().Throw<ArgumentNullException>().And.ParamName.Equals("sectionName");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_LoadConfiguration_InvalidConfigName()
    {
        // arrange act
        Action act = () => ConfigurationBuilder.LoadConfiguration("cacheManager", string.Empty);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("configName");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_LoadConfiguration_SectionDoesNotExist()
    {
        // arrange
        var sectionName = Guid.NewGuid().ToString();

        // act
        Action act = () => ConfigurationBuilder.LoadConfiguration(sectionName, "configName");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No section defined with name " + sectionName + ".");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_LoadConfigurationFile_EmptyCfgFileName()
    {
        // arrange act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(string.Empty, "configName");

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("configFileName");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_LoadConfigurationFile_EmptySectionName()
    {
        // arrange act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile("file", null, "configName");

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("sectionName");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_LoadConfigurationFile_EmptyConfigName()
    {
        // arrange act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile("file", "section", null);

        // assert
        act.Should().Throw<ArgumentException>()
            .And.ParamName.Equals("configName");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_LoadConfigurationFile_NotExistingCfgFileName()
    {
        // arrange
        var fileName = "notexistingconfiguration.config";

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "configName");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuration file not found*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_MissingCacheManagerCfgName()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.missingName.config");

        // act
        var exception = Record.Exception(() => ConfigurationBuilder.LoadConfigurationFile(fileName, "configName"));

        // assert
        exception.Should().NotBeNull();
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_NoSection()
    {
        // arrange
        var fileName = TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.noSection.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "configName");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No section with name * found in file *");
    }

    /* handle definition */

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_MissingDefId()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.missingDefId.config");

        // act
        var exception = Record.Exception(() => ConfigurationBuilder.LoadConfigurationFile(fileName, "configName"));

        // assert
        exception.Should().NotBeNull();
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_InvalidType()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.invalidType.config");

        // act
        var exception = Record.Exception(() => CacheFactory.FromConfiguration<object>(
            ConfigurationBuilder.LoadConfigurationFile(fileName, "configName")));

        // assert
        exception.Should().NotBeNull();
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_InvalidType_NumberOfGenericArgs()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.invalidType.config");

        // act
        var cfg = ConfigurationBuilder.LoadConfigurationFile(fileName, "cacheManager2", "configName");
        Action act = () => CacheFactory.FromConfiguration<string>(cfg);

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cache handle type* should not have any generic arguments*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_InvalidType_HandleType()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.invalidType.config");

        // act
        var cfg = ConfigurationBuilder.LoadConfigurationFile(fileName, "cacheManager4", "configName");

        Action act = () => CacheFactory.FromConfiguration<object>(cfg);

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Configured cache handle does not implement*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_InvalidType_WrongNumberOfGenericTypeArgs()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.invalidType.config");

        // act
        var exception = Record.Exception(() => CacheFactory.FromConfiguration<object>(
            ConfigurationBuilder.LoadConfigurationFile(fileName, "configName")));

        // assert
        exception.Should().NotBeNull();
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_NoHandleDef()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(
                @"/Configuration/configuration.invalid.emptyHandleDefinition.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "configName");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("There are no cache handles defined.");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_CacheManagerWithoutLinkedHandles()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(
                @"/Configuration/configuration.invalid.managerWithoutHandles.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("There are no valid cache handles linked to the cache manager configuration [c1]");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_CacheManagerWithOneInvalidRef()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.InvalidRef.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Referenced cache handle [thisRefIsInvalid] cannot be found in cache handles definition.");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_HandleDefInvalidExpirationMode()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.invalidDefExpMode.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<ConfigurationErrorsException>().WithMessage("*defaultExpirationMode*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_HandleDefInvalidTimeout()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.invalidDefTimeout.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The value of the property 'defaultTimeout' cannot be parsed [20Invalid].");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_InvalidExpirationMode()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.InvalidExpMode.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ThisIsInvalid*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_InvalidEnableStats()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.InvalidEnableStats.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<ConfigurationErrorsException>()
            .WithMessage("*enableStatistics*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_ManagerInvalidTimeout()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.InvalidTimeout.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The value of the property 'timeout' cannot be parsed [thisisinvalid].");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_ManagerInvalidUpdateMode()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.InvalidUpdateMode.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<ConfigurationErrorsException>()
            .WithMessage("*updateMode*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_ExpirationModeWithoutTimeout()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(
                @"/Configuration/configuration.invalid.ExpirationWithoutTimeout.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Expiration mode set without a valid timeout specified for handle [h1]");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_MaxRetriesLessThanOne()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.MaxRetries.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Maximum number of retries must be greater than zero.");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_RetryTimeoutLessThanZero()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.RetryTimeout.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Retry timeout must be greater than or equal to zero.");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_BackplaneNameButNoType()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.backplaneNameNoType.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Backplane type cannot be null if backplane name is specified.");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_BackplaneTypeButNoName()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.backplaneTypeNoName.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c1");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Backplane name cannot be null if backplane type is specified.");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_BackplaneInvalidType()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.backplaneTypeNoName.config");

        // act
        var cfg = ConfigurationBuilder.LoadConfigurationFile(fileName, "invalidType");
        Action act = () => new BaseCacheManager<string>(cfg);

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*does not extend from CacheBackplane*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_BackplaneTypeNotFound()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.backplaneTypeNoName.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "typeNotFound");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Backplane type not found*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_SerializerType_A()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.serializerType.config");

        // act
        Action act = () =>
            CacheFactory.FromConfiguration<object>(ConfigurationBuilder.LoadConfigurationFile(fileName, "c1"));

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must implement " + nameof(ICacheSerializer) + "*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_InvalidCfgFile_SerializerType_B()
    {
        // arrange
        var fileName =
            TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.invalid.serializerType.config");

        // act
        Action act = () => ConfigurationBuilder.LoadConfigurationFile(fileName, "c2");

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*type not found*");
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_CreateConfig_CacheManagerHandleCollection()
    {
        // arrange act
        var col = new CacheManagerHandleCollection()
        {
            Name = "name",
            UpdateMode = CacheUpdateMode.Up,
            EnableStatistics = true,
            MaximumRetries = 10012,
            RetryTimeout = 234,
            BackplaneName = "backplane",
            BackplaneType = typeof(string).AssemblyQualifiedName
        };

        // assert
        col.Name.Should().Be("name");
        col.BackplaneName.Should().Be("backplane");
        col.BackplaneType.Should().Be(typeof(string).AssemblyQualifiedName);
        col.UpdateMode.Should().Be(CacheUpdateMode.Up);
        col.EnableStatistics.Should().BeTrue();
        col.MaximumRetries.Should().Be(10012);
        col.RetryTimeout.Should().Be(234);
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_CreateConfig_CacheManagerHandle()
    {
        // arrange act
        var col = new CacheManagerHandle()
        {
            IsBackplaneSource = true,
            Name = "name",
            ExpirationMode = ExpirationMode.Absolute.ToString(),
            Timeout = "22m",
            RefHandleId = "ref"
        };

        // assert
        col.Name.Should().Be("name");
        col.ExpirationMode.Should().Be(ExpirationMode.Absolute.ToString());
        col.Timeout.Should().Be("22m");
        col.RefHandleId.Should().Be("ref");
        col.IsBackplaneSource.Should().BeTrue();
    }

    [Fact]
    [ReplaceCulture]
    public void Cfg_CreateConfig_CacheHandleDefinition()
    {
        // arrange act
        var col = new CacheHandleDefinition()
        {
            Id = "id",
            HandleType = typeof(string),
            DefaultTimeout = "22m",
            DefaultExpirationMode = ExpirationMode.None
        };

        // assert
        col.Id.Should().Be("id");
        col.HandleType.Should().Be(typeof(string));
        col.DefaultTimeout.Should().Be("22m");
        col.DefaultExpirationMode.Should().Be(ExpirationMode.None);
    }
}