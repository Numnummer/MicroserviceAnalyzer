using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Abstractions.Services;
using MicroserviceAnalyzer.BL.Entities;

namespace MicroserviceAnalyzer.BL.Models.AnalyzerChain.Nlayer;

public class NlayerApiAnalyzer(IProjectService projectService): ChainUnit
{
    /// <summary>
    /// Здесь анализируется, есть ли в данном микросервисе типа NLayer
    /// api-слой и как он реализован: как web-api или grpc.
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        if (request.FileSystem == null) 
            throw new ArgumentNullException(nameof(request.FileSystem));
        
        var apiNode = FindApiProject(request.FileSystem);
        var grpcNode = await FindGrpcProjectAsync(request.FileSystem);
        
        request.ApiInfo.HasWeb=apiNode != null && grpcNode == null;
        request.ApiInfo.HasGrpc=apiNode != null && grpcNode != null;

        if (apiNode != null && grpcNode == null)
        {
            request.ApiInfo.DotnetVersion = await projectService.GetDotnetVersionAsync(apiNode) ?? "net8.0";
            request.AssemblyInfos.Add(new AssemblyInfo(apiNode));
        }
        else if (apiNode != null && grpcNode != null)
        {
            request.ApiInfo.DotnetVersion = await projectService.GetDotnetVersionAsync(grpcNode) ?? "net8.0";
            request.AssemblyInfos.Add(new AssemblyInfo(grpcNode));
        }
        await NextUnitAsync(request);
    }

    private FileSystem.TreeNode? FindApiProject(FileSystem fileSystem)
    {
        foreach (var currentNode in fileSystem.TraverseDfs())
        {
            if(currentNode.Children==null) continue;
            if (currentNode.Children.Any(childNode=>childNode.Name.Split('.').Last() == "csproj")
                && currentNode.Children.Any(childNode=>childNode.Name == "Program.cs")
                && currentNode.Children.Any(childNode=>childNode.Name == "appsettings.json")
                && currentNode.Children.Any(childNode=>childNode.Name == "Properties")
               )
            {
                return currentNode;
            }
        }
        return null;
    }
    
    private async Task<FileSystem.TreeNode?> FindGrpcProjectAsync(FileSystem fileSystem)
    {
        foreach (var currentNode in fileSystem.TraverseDfs())
        {
            if(currentNode.Children==null 
               || currentNode.Children.All(childNode => childNode.Name.Split('.').Last() != "csproj")) continue;
            var projectFile=currentNode.Children.First(childNode => childNode.Name.Split('.').Last() == "csproj");
            var text = await File.ReadAllTextAsync(projectFile.FullPath);
            if(text.Contains(" <Protobuf"))
                return currentNode;
        }
        return null;
    }
}