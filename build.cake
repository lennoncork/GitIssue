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
            Framework = "netcoreapp3.1",
            Runtime = "win10-x64",
            PublishReadyToRun = false,
            PublishTrimmed = false,
            SelfContained = true,
            PublishSingleFile = true,
            Configuration = "Release",
            OutputDirectory = "./.publish/"
        });
    });

Task("Package")
    .IsDependentOn("Test")
    .Does(() =>
    {
        DotNetCorePack("src/GitIssue.sln", new DotNetCorePackSettings
        {
            Configuration = "Release",
            OutputDirectory = "./.packages/"
        });
    });

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Package")
    .IsDependentOn("Publish");

RunTarget(target);