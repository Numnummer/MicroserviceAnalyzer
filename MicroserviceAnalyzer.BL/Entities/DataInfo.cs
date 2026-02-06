namespace MicroserviceAnalyzer.BL.Entities;

public class DataInfo
{
    public bool HasEfCoreContext { get; set; } = false;
    public string DotnetVersion { get; set; } = "net8.0";
}