namespace MicroserviceAnalyzer.BL.Entities;

public class ApiInfo
{
    private bool _hasWeb;
    public bool HasWeb
    {
        get => _hasWeb;
        set => _hasWeb = !_hasGrpc && value;
    }

    private bool _hasGrpc;
    public bool HasGrpc
    {
        get => _hasGrpc;
        set => _hasGrpc = !_hasWeb && value;
    }
    public bool HasSwagger { get; set; }
    public string DotnetVersion { get; set; } = string.Empty;
}