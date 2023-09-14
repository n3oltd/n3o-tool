using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using System.Collections.Generic;

namespace N3O.Tool.Utilities; 

/// <summary>
/// Without this code, package references from the other project are not applied to the package (they are simply ignored).
/// 
/// Many thanks to @WallaceKelly for coming up with a bundling solution (modify the .csproj file with the following parameters):
///
///  <PropertyGroup>
///    <TargetsForTfmSpecificBuildOutput>$(TargetsForTfmSpecificBuildOutput);CopyProjectReferencesToPackage</TargetsForTfmSpecificBuildOutput>
///    <!-- include PDBs in the NuGet package -->
///    <AllowedOutputExtensionsInPackageBuildOutputFolder>$(AllowedOutputExtensionsInPackageBuildOutputFolder);.pdb</AllowedOutputExtensionsInPackageBuildOutputFolder>
///  </PropertyGroup>
///
///  <Target Name="CopyProjectReferencesToPackage" DependsOnTargets="ResolveReferences">
///    <ItemGroup>
///      <BuildOutputInPackage Include="@(ReferenceCopyLocalPaths->WithMetadataValue('ReferenceSourceTarget', 'ProjectReference'))" />
///    </ItemGroup>
///  </Target>
///
///  <!-- + add PrivateAssets="all" to each of your <ProjectReference ...> -->
///
///  Source: https://github.com/nuget/home/issues/3891#issuecomment-459848847
///  
/// </summary>
public static class DotnetPackerStandalone {
    public static void UpdateProject(string projectFileFullPath) {
        var originalXmlStr = File.ReadAllText(projectFileFullPath);

        if (!originalXmlStr.Contains("TargetsForTfmSpecificBuildOutput")) {
            return;
            //throw new Exception("Be sure to use @WallaceKelly's workaround: https://github.com/nuget/home/issues/3891#issuecomment-459848847");
        }

        var newXmlStr = BuildMergedProjFile(projectFileFullPath);

        if (newXmlStr != null) {
            File.WriteAllText(projectFileFullPath, newXmlStr);
        }
    }

    /// <summary>
    /// Creates a csproj file string that includes all the sub-project nuget packages
    /// </summary>
    private static string BuildMergedProjFile(string mainProjectFilePath) {
        var packageReferences = new Dictionary<string, List<PackageReference>>();

        var mainProjXmlStr = File.ReadAllText(mainProjectFilePath);
        var mainProjXmlDoc = XDocument.Parse(mainProjXmlStr);
        var projectReferences = GetAllProjectReferences(mainProjXmlDoc, privateAssetsOnly: true);

        if (!projectReferences.Any()) {
            return null;
            //throw new Exception($"Project file '{mainProjectFilePath}' has no project references with the attribute 'PrivateAssets=\"all\"'.");
        }

        var basePath = Path.GetDirectoryName(mainProjectFilePath);

        AppendSubProjectReferences(basePath, projectReferences, ref packageReferences);

        ValidateNugetPackageVersions(packageReferences);

        var newXmlStr = BuildMergedXmlStr(packageReferences, mainProjXmlStr);

        return newXmlStr;
    }

    /// <summary>
    /// Formats the the new package references and adds them to the previous csproj file text
    /// </summary>
    private static string BuildMergedXmlStr(Dictionary<string, List<PackageReference>> packageReferences,
        string mainProjXmlStr) {
        var uniquePackages = packageReferences
            .SelectMany(pair => pair.Value)
            .GroupBy(pr => pr.PackageName)
            .Where(g => !g.Key.StartsWith("Microsoft.SourceLink"))
            .Select(group => group.First())
            .ToList();

        var uniquePackagesAsStrList = uniquePackages
            .Select(item => $@"<PackageReference Include=""{item.PackageName}"" Version=""{item.Version}"" />")
            .ToList();

        var newLine = "\r\n";

        var uniquePackagesMergedStr =
            "\t<!-- Automatically generated (references from bundled sub-projects) -->"
            + $"{newLine}\t<ItemGroup>"
            + $"{newLine}\t\t" + string.Join("\r\n\t\t", uniquePackagesAsStrList)
            + $"{newLine}\t</ItemGroup>"
            + $"{newLine}";

        var newXmlStr = mainProjXmlStr.Replace(
            "</Project>",
            uniquePackagesMergedStr + $"{newLine}</Project>");

        return newXmlStr;
    }

    /// <summary>
    /// All nuget packages accross sub-projects must share a single version.
    /// If this is not the case, this function will throw an exception and will
    /// point to the project/package/version that is resonsible for this error.
    /// </summary>
    private static void ValidateNugetPackageVersions(Dictionary<string, List<PackageReference>> packageReferences) {
        var packagesAsList = packageReferences
            .SelectMany(pair => pair.Value)
            .GroupBy(pr => pr.PackageName)
            .ToDictionary(
                pair => pair.Key,
                pair => pair.ToList()
            );

        var errors = new List<string>();
        foreach (var packageGroup in packagesAsList) {
            var packageName = packageGroup.Key;
            var packageVersions = packageGroup.Value
                .Select(pr => pr.Version)
                .Distinct()
                .ToList();
            if (packageVersions.Count > 1) {
                var versionsPerProject = packageReferences
                    .Where(pair => pair.Value.Any(pr => pr.PackageName == packageName))
                    .ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value.Where(pr => pr.PackageName == packageName).Single().Version
                    );
                var error =
                    $"Package '{packageName}' has {packageVersions.Count} different versions ({string.Join(", ", packageVersions)}): {string.Join(", ", versionsPerProject.Select(pair => $"'{pair.Key}':{pair.Value}"))}";
                errors.Add(error);
            }
        }

        if (errors.Any())
            throw new Exception($"Nuget package errors:\r\n{string.Join("\r\n", errors)}");
    }

    /// <summary>
    /// Recursive function that loops through all sub-projects and collects all package references
    /// </summary>
    private static void AppendSubProjectReferences(string basePath, List<string> projectReferences,
        ref Dictionary<string, List<PackageReference>> packageReferences) {
        if (!Host.IsWindows) {
            projectReferences = projectReferences.Select(x => x.Replace("\\", "/")).ToList();
        }

        foreach (var projectReference in projectReferences) {
            var fileName = Path.GetFileName(projectReference);
            if (packageReferences.ContainsKey(fileName))
                continue;

            var subProjFilePath = Path.GetFullPath(Path.Combine(basePath, projectReference));
            var subProjXmlStr = File.ReadAllText(subProjFilePath);
            var subProjXmlDoc = XDocument.Parse(subProjXmlStr);
            var partialPackageReferences = GetAllPackageReferences(subProjXmlDoc);
            packageReferences.Add(fileName, partialPackageReferences);

            var subBasePath = Path.GetDirectoryName(subProjFilePath);

            var subProjectReferences = GetAllProjectReferences(subProjXmlDoc, privateAssetsOnly: false);
            foreach (var subProjectReference in subProjectReferences)
                AppendSubProjectReferences(subBasePath, subProjectReferences, ref packageReferences);
        }
    }

    /// <summary>
    /// Parses and collects all package project references from the csproj document
    /// </summary>
    private static List<string> GetAllProjectReferences(XDocument doc, bool privateAssetsOnly)
        => doc.XPathSelectElements("//ProjectReference")
            .Where(pr => privateAssetsOnly ? IsPrivateAsset(pr) : true)
            .Select(pr => pr.Attribute("Include").Value)
            .ToList();

    /// <summary>
    /// Tells if a project reference has PrivateAssets="all".
    /// Those are the only top-level project references that will be bundled.
    /// On the other hand, all sub-project references (that are not referenced at the top level) will be bundled.
    /// </summary>
    private static bool IsPrivateAsset(XElement pr)
        => pr.Attribute("PrivateAssets") != null && pr.Attribute("PrivateAssets").Value.ToLower() == "all";

    /// <summary>
    /// Parses and collects all package package references from the csproj document
    /// </summary>
    private static List<PackageReference> GetAllPackageReferences(XDocument doc)
        => doc.XPathSelectElements("//PackageReference")
            .Select(pr => new PackageReference {
                PackageName = pr.Attribute("Include").Value,
                Version = pr.Attribute("Version").Value
            }).ToList();
}

public class PackageReference {
    public string PackageName { get; set; }
    public string Version { get; set; }
}