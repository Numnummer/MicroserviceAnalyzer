using System.Text;
using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.Entities;

public class MicroserviceInfo
{
    public string Name { get; set; } = string.Empty;
    public ArchitectureInfo Architecture { get; set; } = new();
    public FileSystem? FileSystem { get; set; }
    
    [Obsolete]
    public List<string> Keywords { get; set; } = [];
    
    public ApiInfo ApiInfo { get; set; } = new();
    public DataInfo DataInfo { get; set; } = new();
    public StringBuilder Script { get; set; } = new();
    
}