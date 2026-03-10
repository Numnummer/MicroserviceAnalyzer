using System;
using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.Entities;

public class AssemblyInfo
{
    public FileSystem.TreeNode? AssemblyTreeNode { get; set; }
    public string[] NugetPackages { get; set; } = [];
    public AssemblyInfo(FileSystem.TreeNode assemblyTreeNode)
    {
        AssemblyTreeNode = assemblyTreeNode;
    }
}
