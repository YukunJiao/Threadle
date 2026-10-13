using Threadle.Core.Utilities;

namespace Threadle.Tests;

/// <summary>
/// Tests touching the 'maxthreadslimit' limit run serially, after all other tests: other test classes
/// set 'maxthreads' in parallel, which would fail if a limit were active at the same time.
/// </summary>
[CollectionDefinition("MaxThreadsLimit", DisableParallelization = true)]
public class MaxThreadsLimitCollection { }

[Collection("MaxThreadsLimit")]
public class AppConfigTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("threadle-appconfig-").FullName;

    public void Dispose()
    {
        UserSettings.MaxThreadsLimit = -1;
        UserSettings.MaxDegreeOfParallelism = -1;
        Directory.Delete(_dir, true);
    }

    private string WriteConfig(string content)
    {
        string path = Path.Combine(_dir, AppConfig.FileName);
        File.WriteAllText(path, content);
        return path;
    }

    // ── Load ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Load_MissingFile_KeepsDefaults()
    {
        var warnings = AppConfig.Load(Path.Combine(_dir, AppConfig.FileName));
        Assert.Empty(warnings);
        Assert.Null(AppConfig.LoadedPath);
        Assert.Equal(-1, UserSettings.MaxThreadsLimit);
        Assert.Equal(-1, UserSettings.MaxDegreeOfParallelism);
    }

    [Fact]
    public void Load_LimitAndDefault_Applied()
    {
        var path = WriteConfig("{ \"maxthreadslimit\": 1, \"defaultmaxthreads\": 1 }");
        var warnings = AppConfig.Load(path);
        Assert.Empty(warnings);
        Assert.Equal(path, AppConfig.LoadedPath);
        Assert.Equal(1, UserSettings.MaxThreadsLimit);
        Assert.Equal(1, UserSettings.MaxDegreeOfParallelism);
    }

    [Fact]
    public void Load_OnlyMaxThreadsLimit_DefaultIsLimit()
    {
        var warnings = AppConfig.Load(WriteConfig("{ \"maxthreadslimit\": 1 }"));
        Assert.Empty(warnings);
        Assert.Equal(1, UserSettings.MaxDegreeOfParallelism);
    }

    [Fact]
    public void Load_OnlyDefaultMaxThreads_NoLimit()
    {
        var warnings = AppConfig.Load(WriteConfig("{ \"defaultmaxthreads\": 1 }"));
        Assert.Empty(warnings);
        Assert.Equal(-1, UserSettings.MaxThreadsLimit);
        Assert.Equal(1, UserSettings.MaxDegreeOfParallelism);
    }

    [Fact]
    public void Load_DefaultAboveLimit_WarnsAndUsesLimit()
    {
        var warnings = AppConfig.Load(WriteConfig("{ \"maxthreadslimit\": 1, \"defaultmaxthreads\": 2 }"));
        Assert.Single(warnings);
        Assert.Equal(1, UserSettings.MaxDegreeOfParallelism);
    }

    [Fact]
    public void Load_CommentsTrailingCommasAndCase_Accepted()
    {
        var warnings = AppConfig.Load(WriteConfig("{\n  // set by IT\n  \"MaxThreadsLimit\": 1,\n}"));
        Assert.Empty(warnings);
        Assert.Equal(1, UserSettings.MaxThreadsLimit);
    }

    [Theory]
    [InlineData("{ \"maxthreadslimit\": 0 }")]
    [InlineData("{ \"maxthreadslimit\": -2 }")]
    [InlineData("{ \"maxthreadslimit\": \"4\" }")]
    [InlineData("{ \"maxthreadslimit\": 2.5 }")]
    [InlineData("{ \"unknownsetting\": 4 }")]
    public void Load_InvalidEntry_WarnsAndIgnoresEntry(string content)
    {
        var warnings = AppConfig.Load(WriteConfig(content));
        Assert.Single(warnings);
        Assert.Equal(-1, UserSettings.MaxThreadsLimit);
        Assert.NotNull(AppConfig.LoadedPath);
    }

    [Theory]
    [InlineData("{ \"maxthreadslimit\": ")]
    [InlineData("[1, 2]")]
    public void Load_MalformedFile_WarnsAndIgnoresFile(string content)
    {
        var warnings = AppConfig.Load(WriteConfig(content));
        Assert.Single(warnings);
        Assert.Null(AppConfig.LoadedPath);
        Assert.Equal(-1, UserSettings.MaxThreadsLimit);
    }

    // ── Set("maxthreads") with a ceiling ────────────────────────────────────────

    [Fact]
    public void SetMaxThreads_AboveLimit_Fails()
    {
        UserSettings.MaxThreadsLimit = 1;
        var result = UserSettings.Set("maxthreads", 2);
        Assert.False(result.Success);
        Assert.Equal("InvalidValue", result.Code);
    }

    [Fact]
    public void SetMaxThreads_AtLimit_Succeeds()
    {
        UserSettings.MaxThreadsLimit = 1;
        var result = UserSettings.Set("maxthreads", 1);
        Assert.True(result.Success);
        Assert.Equal(1, UserSettings.MaxDegreeOfParallelism);
    }

    [Fact]
    public void SetMaxThreads_Unconstrained_WithLimit_UsesLimit()
    {
        UserSettings.MaxThreadsLimit = 1;
        var result = UserSettings.Set("maxthreads", -1);
        Assert.True(result.Success);
        Assert.Equal(1, UserSettings.MaxDegreeOfParallelism);
    }

    [Fact]
    public void EffectiveMaxThreadsLimit_NoLimit_IsProcessorCount()
    {
        UserSettings.MaxThreadsLimit = -1;
        Assert.Equal(Environment.ProcessorCount, UserSettings.EffectiveMaxThreadsLimit);
    }
}