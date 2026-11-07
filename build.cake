#tool dotnet:?package=dotnet-reportgenerator-globaltool&version=5.5.11

#nullable enable

var target = Argument("target", "Default");

var configuration = Argument("configuration", "Release");

var coverageDirectory = MakeAbsolute(Directory("./coverage"));

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

Task("Format")
    .Does(() =>
    {
        DotNetFormat("./src/GitIssue.slnx", new DotNetFormatSettings
        {
            VerifyNoChanges = true,
        });
    });

Task("Analyzers")
    .Does(() =>
    {
        DotNetFormat("./src/GitIssue.slnx", new DotNetFormatSettings
        {
            ArgumentCustomization = args => args.Append("analyzers").Append("--severity").Append("info"),
            VerifyNoChanges = true,
        });
    });

Task("Test")
    .DoesForEach(() => GetFiles("./src/**/*Tests.csproj"), testProject =>
    {
        DotNetTest(testProject.FullPath, new DotNetTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
        });
    });

Task("Coverage")
    .Does(() =>
    {
        CleanDirectory(coverageDirectory);
    })
    .DoesForEach(() => GetFiles("./src/**/*Tests.csproj"), testProject =>
    {
        var coverageFile = coverageDirectory.Combine($"{testProject.GetFilenameWithoutExtension()}.cobertura.xml");
        var arguments = new ProcessArgumentBuilder()
            .Append("collect")
            .Append("--settings")
            .AppendQuoted(MakeAbsolute(File("./coverage.settings")).FullPath)
            .Append("dotnet")
            .Append("test")
            .AppendQuoted(testProject.FullPath)
            .Append("--configuration")
            .Append(configuration)
            .Append("--no-build")
            .Append("--output-format")
            .Append("cobertura")
            .Append("--output")
            .AppendQuoted(coverageFile.FullPath);

        DotNetTool("./", "dotnet-coverage", arguments);
    });

Task("Report")
    .IsDependentOn("Coverage")
    .Does(() =>
    {
        var coverageFiles = GetFiles($"{coverageDirectory}/*.cobertura.xml");
        if (!coverageFiles.Any())
        {
            throw new CakeException($"No coverage files were generated in {coverageDirectory}.");
        }

        ReportGenerator(
            coverageFiles,
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
    .IsDependentOn("Format")
    .IsDependentOn("Analyzers")
    .IsDependentOn("Test")
    .IsDependentOn("Coverage")
    .IsDependentOn("Report")
    .IsDependentOn("Package")
    .IsDependentOn("Publish");

RunTarget(target);