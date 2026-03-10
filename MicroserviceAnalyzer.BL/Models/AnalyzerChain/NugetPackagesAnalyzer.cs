using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;

namespace MicroserviceAnalyzer.BL.Models.AnalyzerChain;

public class NugetPackagesAnalyzer : ChainUnit
{
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        foreach (var assemblyInfo in request.AssemblyInfos)
        {
            assemblyInfo.NugetPackages = await
                GetNugetPackageReferencesForAssemblyAsync(assemblyInfo.AssemblyTreeNode);
        }
        await NextUnitAsync(request);
    }
    private FileSystem.TreeNode? GetCsprojFile(FileSystem.TreeNode assemblyFolder)
        => assemblyFolder?.Children?.FirstOrDefault(n => n.Name.Split('.').Last() == "csproj");
    private async Task<string[]> GetNugetPackageReferencesForAssemblyAsync(FileSystem.TreeNode? assemblyFolder)
    {
        if (assemblyFolder == null) return [];
        var csprojFile = GetCsprojFile(assemblyFolder);
        if (csprojFile == null) return [];
        var content = await File.ReadAllTextAsync(csprojFile.FullPath);
        var doc = XDocument.Parse(content);
        
        var packageReferences = doc.Descendants("PackageReference")
            .Select(x => x.Attribute("Include")?.Value)
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => name!)
            .ToList();
        
        return [.. packageReferences];
    }
}
