namespace MicroserviceAnalyzer.BL.Abstractions.Services;

public interface IScriptService
{
    Task RunScriptAsync(string script, string workingDirectory);
}