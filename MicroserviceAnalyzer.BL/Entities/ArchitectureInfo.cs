using MicroserviceAnalyzer.BL.Variations;

namespace MicroserviceAnalyzer.BL.Models;

public class ArchitectureInfo
{
    public Architecture Architecture { get; set; }
    public ApiVariation ApiVariation { get; set; }
    public List<string> ArchitectureVariations { get; set; } = [];
}