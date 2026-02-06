using MicroserviceAnalyzer.BL.Entities;

namespace MicroserviceAnalyzer.BL.Abstractions.Services;

public interface IMicroserviceService
{
    Task<MicroserviceInfo> AnalyzeAsync();
    Task<string> BuildScriptAsync(MicroserviceInfo microserviceInfo);
}