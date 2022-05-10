using System;
using System.IO;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.NuGet.Push;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Common.Tools.DotNet.Restore;
using Cake.Common.Tools.DotNet.Test;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;

return new CakeHost()
    .InstallTool(new Uri("dotnet:?package=GitVersion.Tool&version=5.10.1"))
    .UseContext<BuildContext>()
    .Run(args);

public static class Constants
{
    public const string Release = "Release";
    public const string ProjectName = "GSoft.Extensions.Configuration.Substitution";

    public static readonly string SourceDirectoryPath = Path.Combine("..", "src");
    public static readonly string OutputDirectoryPath = Path.Combine("..", ".output");
    public static readonly string SolutionPath = Path.Combine(SourceDirectoryPath, ProjectName + ".sln");
    public static readonly string MainProjectPath = Path.Combine(SourceDirectoryPath, ProjectName, ProjectName + ".csproj");
}

public class BuildContext : FrostingContext
{
    public BuildContext(ICakeContext context) : base(context)
    {
        this.NugetApiKey = context.Argument("nuget-api-key", string.Empty);
        this.NugetSource = context.Argument("nuget-source", string.Empty);
    }

    public DotNetMSBuildSettings MSBuildSettings { get; } = new DotNetMSBuildSettings();

    public string NugetApiKey { get; }

    public string NugetSource { get; }

    public void AddMSBuildSetting(string name, string value, bool log = false)
    {
        if (log)
        {
            this.Log.Information(name + ": " + value);
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            this.MSBuildSettings.Properties[name] = new[] { value };
        }
    }
}

[TaskName("Clean")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var objGlobPath = Path.Combine(Constants.SourceDirectoryPath, "*", "obj");
        var binGlobPath = Path.Combine(Constants.SourceDirectoryPath, "*", "bin");

        context.CleanDirectories(Constants.OutputDirectoryPath);
        context.CleanDirectories(objGlobPath);
        context.CleanDirectories(binGlobPath);
    }
}

[TaskName("GitVersion")]
public sealed class GitVersionTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var gitVersion = context.GitVersion();

        context.AddMSBuildSetting("Version", gitVersion.NuGetVersion, log: true);
        context.AddMSBuildSetting("VersionPrefix", gitVersion.MajorMinorPatch, log: true);
        context.AddMSBuildSetting("VersionSuffix", gitVersion.PreReleaseTag, log: true);
        context.AddMSBuildSetting("PackageVersion", gitVersion.FullSemVer, log: true);
        context.AddMSBuildSetting("InformationalVersion", gitVersion.InformationalVersion, log: true);
        context.AddMSBuildSetting("AssemblyVersion", gitVersion.AssemblySemVer, log: true);
        context.AddMSBuildSetting("FileVersion", gitVersion.AssemblySemFileVer, log: true);
        context.AddMSBuildSetting("RepositoryBranch", gitVersion.BranchName, log: true);
        context.AddMSBuildSetting("RepositoryCommit", gitVersion.Sha, log: true);
    }
}

[TaskName("Restore")]
[IsDependentOn(typeof(CleanTask))]
[IsDependentOn(typeof(GitVersionTask))]
public sealed class RestoreTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context) => context.DotNetRestore(Constants.SolutionPath, new DotNetRestoreSettings
    {
        MSBuildSettings = context.MSBuildSettings,
    });
}

[TaskName("Build")]
[IsDependentOn(typeof(RestoreTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.AddMSBuildSetting("Deterministic", "true");
        context.AddMSBuildSetting("ContinuousIntegrationBuild", "true");

        context.DotNetBuild(Constants.SolutionPath, new DotNetBuildSettings
        {
            Configuration = Constants.Release,
            MSBuildSettings = context.MSBuildSettings,
            NoRestore = true,
            NoLogo = true,
        });
    }
}

[TaskName("Test")]
[IsDependentOn(typeof(BuildTask))]
public sealed class TestTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        foreach (var testProjectFilePath in context.GetFiles(Path.Combine(Constants.SourceDirectoryPath, "*", "*.Tests.csproj")))
        {
            context.DotNetTest(testProjectFilePath.FullPath, new DotNetTestSettings
            {
                Configuration = Constants.Release,
                Loggers = new[] { "console;verbosity=detailed" },
                NoBuild = true,
                NoLogo = true,
            });
        }
    }
}

[TaskName("Pack")]
[IsDependentOn(typeof(TestTask))]
public sealed class PackTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context) => context.DotNetPack(Constants.MainProjectPath, new DotNetPackSettings
    {
        Configuration = Constants.Release,
        MSBuildSettings = context.MSBuildSettings,
        OutputDirectory = Constants.OutputDirectoryPath,
        NoBuild = true,
        NoRestore = true,
        NoLogo = true,
    });
}

[TaskName("Push")]
[IsDependentOn(typeof(PackTask))]
public sealed class PushTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        foreach (var packageFilePath in context.GetFiles(Path.Combine(Constants.OutputDirectoryPath, "*.nupkg")))
        {
            context.DotNetNuGetPush(packageFilePath, new DotNetNuGetPushSettings
            {
                ApiKey = context.NugetApiKey,
                Source = context.NugetSource,
                IgnoreSymbols = false
            });
        }
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PackTask))]
public sealed class DefaultTask : FrostingTask
{
}