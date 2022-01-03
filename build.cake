var target = Argument("target", "Default");

var configuration = Argument("configuration", "Release");

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
    {
        CleanDirectory($"./src/GitIssue/.output/");
    });

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreBuild("./src/GitIssue.sln", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
        });
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCoreTest("./src/GitIssue.sln", new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
        });
    });

Task("Publish")
    .IsDependentOn("Test")
    .Does(() =>
    {
        DotNetCorePublish("src/GitIssue.Tool/GitIssue.Tool.csproj", new DotNetCorePublishSettings
        {
            Framework = "net5.0",
            Runtime = "win10-x64",
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
        DotNetCorePack("src/GitIssue.sln", new DotNetCorePackSettings
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