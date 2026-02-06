using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;

namespace MicroserviceAnalyzer.BL.AnalyzerChain.Nlayer;

public class NlayerNameAnalyzer : ChainUnit
{
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        var firstLevelNodes = request.FileSystem?.Root.Children;
        if (firstLevelNodes == null || firstLevelNodes.Count == 0)
            throw new Exception("Не найдены файлы микросервиса");
        var firstDirectoryNode = firstLevelNodes.FirstOrDefault(node=>node.IsDirectory);
        if (firstDirectoryNode == null)
            throw new Exception("Не найдены папки микросервиса");
        request.Name = firstDirectoryNode.Name.Contains(".") 
            ? firstDirectoryNode.Name.Split(".").First() 
            : firstDirectoryNode.Name;
        await NextUnitAsync(request);
    }
}