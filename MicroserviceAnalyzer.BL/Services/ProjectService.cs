using MicroserviceAnalyzer.BL.Abstractions.Services;
using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.Services;

public class ProjectService : IProjectService
{
    public async Task<string?> GetDotnetVersionAsync(FileSystem.TreeNode projectNode)
    {
        if(projectNode.Children == null || projectNode.Children.Count == 0)
            return null;
        var projectFile=projectNode.Children.FirstOrDefault(childNode => childNode.Name.Split('.').Last() == "csproj");
        if(projectFile == null)
            return null;
        var content = await File.ReadAllTextAsync(projectFile.FullPath);
        string[] separators=["<TargetFramework>", "</TargetFramework>"];
        return content.Split(separators, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1);
    }
}