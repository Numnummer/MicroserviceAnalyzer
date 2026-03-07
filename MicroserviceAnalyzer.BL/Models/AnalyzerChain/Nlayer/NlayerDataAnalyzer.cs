using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Abstractions.Services;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Triggers;

namespace MicroserviceAnalyzer.BL.Models.AnalyzerChain.Nlayer;

public class NlayerDataAnalyzer(IProjectService projectService):ChainUnit
{
    private readonly DataLayerTrigger _dataLayerTrigger=new();
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        if (request.FileSystem == null)
            throw new ArgumentNullException(nameof(request.FileSystem));
        var dataLayerNode = FindDataProjectRecursive(request.FileSystem);
        if (dataLayerNode != null)
        {
            request.DataInfo.HasEfCoreContext = HasEfCoreDbContext(request.FileSystem, dataLayerNode);
            request.DataInfo.DotnetVersion = await projectService.GetDotnetVersionAsync(dataLayerNode) ?? "net8.0";
            request.DataInfo.EfCoreProvider = GetEfCoreProvider(dataLayerNode);
        }
        
        await NextUnitAsync(request);
    }

    private FileSystem.TreeNode? FindDataProjectRecursive(FileSystem fileSystem)
    {
        foreach (var currentNode in fileSystem.TraverseDfs())
        {
            if (currentNode.Children == null || currentNode.Children.Count == 0)
                continue;
            _dataLayerTrigger.DataLayerNode = currentNode;
            if (_dataLayerTrigger.GetTriggerPercentage() == 100)
                return currentNode;
        }
        return null;
    }

    private bool HasEfCoreDbContext(FileSystem fileSystem, FileSystem.TreeNode dataLayerNode)
    {
        foreach (var currentNode in fileSystem.TraverseDfs(dataLayerNode, true))
        {
            var hasCsFileChild = currentNode.Children?.All(n => n.Name.Split('.').Last() != "cs") 
                                 ?? false;
            if (hasCsFileChild && currentNode.IsDirectory)
                continue;

            if(HasFileEfCoreDbContext(currentNode))
                return true;
        }
        return false;
    }

    private EfCoreProvider? GetEfCoreProvider(FileSystem.TreeNode dataLayerNode)
    {
        var projectFile = dataLayerNode?.Children?.FirstOrDefault(n => n.Name.Split('.').Last() == "csproj");
        if (projectFile != null)
        {
            // TODO: подумать может ли быть много разных провайдеров в проекте и как это обрабатывать.
            var content = File.ReadAllText(projectFile.FullPath);
            var pgsqlTrigger = "PackageReference Include=\"Npgsql.EntityFrameworkCore.PostgreSQL\"";
            var sqlservTrigger = "PackageReference Include=\"Microsoft.EntityFrameworkCore.SqlServer\"";
            var sqliteTrigger = "PackageReference Include=\"Microsoft.EntityFrameworkCore.Sqlite\"";
            if (content.Contains(pgsqlTrigger)) return EfCoreProvider.psql;
            if (content.Contains(sqlservTrigger)) return EfCoreProvider.sqlserv;
            if (content.Contains(sqliteTrigger)) return EfCoreProvider.sqlite;
        }
        return null;
    }

    private bool HasFileEfCoreDbContext(FileSystem.TreeNode fileNode)
    {
        var file = fileNode.Children != null
            ? fileNode.Children.First(n => n.Name.Split('.').Last() == "cs")
            : fileNode;
        var content = File.ReadAllText(file.FullPath);
        return content.Contains("using Microsoft.EntityFrameworkCore;")
                           && (content.Contains(" DbContext") || content.Contains(":DbContext"));
    }
}