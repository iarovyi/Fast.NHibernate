#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=ILRepack"

using Cake.Common.Tools.GitVersion;
using IoPath = System.IO.Path;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
string buildDir = Directory("./src/Fast.NHibernate/bin") + Directory(configuration);
string outputDir = Directory("./output");
string relativeSlnPath = "./src/Fast.NHibernate.sln";
string primaryDllName = "Fast.NHibernate.dll";
string nugetDir = "./nuget";

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
    CleanDirectory(outputDir);
    CleanDirectory(nugetDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(relativeSlnPath);
});

Task("Update-Assembly-Info")
    .Does(() =>
{
    GitVersion version = GitVersion(new GitVersionSettings { UpdateAssemblyInfo = true });
});

Task("Build")
    .IsDependentOn("Update-Assembly-Info")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      MSBuild(relativeSlnPath, settings => settings.SetConfiguration(configuration));
    }
    else
    {
      XBuild(relativeSlnPath, settings => settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    //TODO: fix xunit runner bug
    //var testAssemblies = GetFiles("./src/**/bin/Release/*.Specs.dll");
    //XUnit2(testAssemblies);
});

Task("Create-Nuget-Package")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
     GitVersion version = GitVersion(new GitVersionSettings());
     var nuGetPackSettings   = new NuGetPackSettings {
                                Version                 = version.AssemblySemVer,
                                NoPackageAnalysis       = true,
                                Files                   = new [] {
                                                                    new NuSpecContent {Source = buildDir + @"/Fast.NHibernate.dll", Target = "lib/net45" },
                                                                 },
                                BasePath                = ".",
                                OutputDirectory         = nugetDir
                            };

     FilePathCollection nuspecFiles = GetFiles("./**/Fast.NHibernate.nuspec");
     Information("Generating nuget with version " + nuGetPackSettings.Version);
     NuGetPack(nuspecFiles, nuGetPackSettings);
});

Task("Push-Nuget-Package")
    .IsDependentOn("Create-Nuget-Package")
    .Does(() =>
{
    string semVersion = GitVersion(new GitVersionSettings()).SemVer;
    ConvertableFilePath package = Directory(nugetDir) + File("Fast.NHibernate." + semVersion + ".nupkg");

    Information("Publishing nuget package " + package);
    NuGetPush(package, new NuGetPushSettings {
        Source = "https://api.nuget.org/v3/index.json",
        ApiKey = EnvironmentVariable("NugetApiKey")
    });
});

Task("Default").IsDependentOn("Run-Unit-Tests");
Task("Package").IsDependentOn("Create-Nuget-Package");
Task("Publish").IsDependentOn("Push-Nuget-Package");
RunTarget(target);
