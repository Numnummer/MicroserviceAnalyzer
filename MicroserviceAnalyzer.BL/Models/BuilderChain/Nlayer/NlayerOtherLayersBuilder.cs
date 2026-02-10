using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Models;
using MicroserviceAnalyzer.BL.Triggers;

namespace MicroserviceAnalyzer.BL.BuilderChain.Nlayer;

public class NlayerOtherLayersBuilder:ChainUnit
{
    private readonly string[] _otherLayers = ["application"];
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        if (request.FileSystem != null)
        {
            foreach (var layer in _otherLayers)
            {
                var node=FindOtherLayerProjectRecursive(request.FileSystem.Root, layer);
                if (node != null)
                    request.Script.Append(GetOtherLayerCreateProjectScript(node, request.ApiInfo.DotnetVersion));
            }
        }
        await NextUnitAsync(request);
    }
    private FileSystem.TreeNode? FindOtherLayerProjectRecursive(FileSystem.TreeNode currentNode, string layer)
    {
        if (currentNode.Children == null || currentNode.Children.Count == 0)
            return null;
        return currentNode.Name.ToLower().Contains(layer)
            ? currentNode 
            : currentNode.Children.Where(n => n.IsDirectory)
                .Select(n=>FindOtherLayerProjectRecursive(n, layer))
                .FirstOrDefault();
    }

    private string GetOtherLayerCreateProjectScript(FileSystem.TreeNode node, string dotnetVersion)
    {
        var name = node.Name.Contains('.') ? "$name." + node.Name.Split('.').Last() : "$name";
        return $"dotnet new classlib --name {name}\ndotnet sln $name.sln add {name}\ncd {name}\nproj_version=\"{dotnetVersion}\"\nfind . -name \"{name}.csproj\" | while read -r file; do\n    if _ -f \"$file\" _; then\n        sed -i \"s|<TargetFramework>.*</TargetFramework>|<TargetFramework>$proj_version</TargetFramework>|g\" \"$file\"\n    fi\ndone\ncd ..\n";
    }
}