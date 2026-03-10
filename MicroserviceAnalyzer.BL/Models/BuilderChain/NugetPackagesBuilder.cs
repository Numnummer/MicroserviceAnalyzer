using System;
using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Helpers;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace MicroserviceAnalyzer.BL.Models.BuilderChain;

public class NugetPackagesBuilder : ChainUnit
{
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        request.Script.Append("\n#ignore_for_temp_start\n");
        foreach (var assemblyInfo in request.AssemblyInfos)
        {
            if (assemblyInfo.AssemblyTreeNode == null) continue;
            foreach (var package in assemblyInfo.NugetPackages)
            {
                var packageVersions = await GetPackageVersions(package);
                string mainScript = $@"
for ver in {string.Join(' ', packageVersions)}; do
        dotnet add package {package} --version $ver && break
done";
                var cdPathComponents = MicroserviceHelper.GetPathComponentsRelativeToMicroservice(assemblyInfo.AssemblyTreeNode.FullPath);
                var cdScript = $"\ncd {string.Join(Path.DirectorySeparatorChar, cdPathComponents)}\n";
                var reverseCdScript = $"\ncd {string.Concat(Enumerable.Repeat("../", cdPathComponents.Length))}\n";
                var script = cdScript + mainScript + reverseCdScript;
                request.Script.Append(script);
            }

        }
        request.Script.Append("\n#ignore_for_temp_end\n");
        await NextUnitAsync(request);
    }
    private async Task<List<NuGetVersion>> GetPackageVersions(string packageId)
    {
        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        var resource = await repository.GetResourceAsync<PackageMetadataResource>();
        
        var searchMetadata = await resource.GetMetadataAsync(
            packageId,
            includePrerelease: true,
            includeUnlisted: false,
            new SourceCacheContext(),
            NullLogger.Instance,
            CancellationToken.None);
        
        return searchMetadata
            .Select(m => m.Identity.Version)
            .OrderByDescending(v => v)
            .ToList();
    }
}
