using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.Abstractions.Services;

public interface IProjectService
{
    Task<string?> GetDotnetVersionAsync(FileSystem.TreeNode projectNode);
}