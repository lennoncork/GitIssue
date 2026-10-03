#addin nuget:?package=Cake.Coverlet&version=6.0.1
#tool dotnet:?package=dotnet-reportgenerator-globaltool&version=5.5.11

#nullable enable

var target = Argument("target", "Default");

var configuration = Argument("configuration", "Release");

var coverageDirectory = MakeAbsolute(Directory("./coverage"));

var coreCoverageThreshold = 80U;

var toolCoverageThreshold = 60U;

Task("Clean")
    .Does(() =>
    {
        CleanDirectory($"./src/GitIssue/.output/");
    });

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetBuild("./src/GitIssue.slnx", new DotNetBuildSettings
        {
            Configuration = configuration,
        });
    });

Task("Test")
    .IsDependentOn("Build")
    .DoesForEach(() => GetFiles("./src/**/*Tests.csproj"), testProject =>
    {
        DotNetTest(testProject.FullPath, new DotNetTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
        });
    });

Task("CoverageTests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        CleanDirectory(coverageDirectory);
    })
    .DoesForEach(() => GetFiles("./src/**/*Tests.csproj"), testProject =>
    {
        bool isCoreTestProject = testProject.GetFilenameWithoutExtension() == "GitIssue.Tests";
        string module = isCoreTestProject ? "GitIssue" : "GitIssue.Tool";
        uint threshold = isCoreTestProject ? coreCoverageThreshold : toolCoverageThreshold;
        CoverletSettings coverlet = new CoverletSettings
        {
            CollectCoverage = true,
            CoverletOutputName = $"Coverage.{testProject.GetFilenameWithoutExtension()}.coverlet",
            CoverletOutputFormat = CoverletOutputFormat.json | CoverletOutputFormat.cobertura,
            CoverletOutputDirectory = coverageDirectory,
            Threshold = threshold,
        }
            .WithInclusion($"[{module}]*")
            .WithThresholdType(ThresholdType.Line);

        DotNetTest(testProject.FullPath, new DotNetTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
        }, coverlet);
    });

Task("Coverage")
    .IsDependentOn("CoverageTests")
    .Does(() =>
    {
        ReportGenerator(
            GetFiles($"{coverageDirectory}/Coverage.*.coverlet"),
            coverageDirectory.Combine("CoverageReport"),
            new ReportGeneratorSettings
            {
                ReportTypes = new[] { ReportGeneratorReportType.MarkdownSummary },
            });
    });

Task("Publish")
    .IsDependentOn("Test")
    .Does(() =>
    {
        DotNetPublish("src/GitIssue.Tool/GitIssue.Tool.csproj", new DotNetPublishSettings
        {
            Framework = "net10.0",
            Runtime = "win-x64",
            PublishReadyToRun = false,
            PublishTrimmed = false,
            SelfContained = true,
            PublishSingleFile = false,
            Configuration = "Release",
            OutputDirectory = "./.publish/"
        });
    });

Task("Package")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetPack("src/GitIssue.slnx", new DotNetPackSettings
        {
            Configuration = "Release",
            OutputDirectory = "./.packages/",
            NoBuild = true,
        });
    });

Task("Push")
    .IsDependentOn("Package")
    .Does(() => 
{
    // Make sure that there is an API key.
    var apiKey =  EnvironmentVariable("NUGET_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey)) {
        throw new CakeException("No NuGet API key specified.");
    }

    foreach(var file in GetFiles("./.packages/*.nupkg"))
    {
        Information("Publishing {0}...", file.FullPath);
        NuGetPush(file, new NuGetPushSettings {
            ApiKey = apiKey,
            Source = "https://api.nuget.org/v3/index.json"
        });
    }
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Package")
    .IsDependentOn("Publish");

RunTarget(target);