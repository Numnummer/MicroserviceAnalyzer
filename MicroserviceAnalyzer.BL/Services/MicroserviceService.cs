using MicroserviceAnalyzer.BL.Abstractions.Services;
using MicroserviceAnalyzer.BL.AnalyzerChain;
using MicroserviceAnalyzer.BL.AnalyzerChain.Nlayer;
using MicroserviceAnalyzer.BL.BuilderChain.Nlayer;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Helpers;
using MicroserviceAnalyzer.BL.Models.AnalyzerChain;
using MicroserviceAnalyzer.BL.Models.AnalyzerChain.Nlayer;
using MicroserviceAnalyzer.BL.Variations;

namespace MicroserviceAnalyzer.BL.Services;

/// <summary>
/// Тут происходит обработка микросервиса.
/// </summary>
public class MicroserviceService:IMicroserviceService,IDisposable
{
    public async Task<MicroserviceInfo> AnalyzeAsync()
    {
        var microserviceInfo = new MicroserviceInfo();
        var baseUnit = new AnalyzeArchitectureUnit();
        await baseUnit.HandleRequestAsync(microserviceInfo);
        
        // Строим дерево вариантов цепочки обязанности
        switch (microserviceInfo.Architecture.Architecture)
        {
            case Architecture.NLayer:
                await BuildAndRunNlayerAnalyzersAsync(microserviceInfo);
                break;
            case Architecture.Mvc:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        DeleteAnalyzedMicroservice();
        
        return microserviceInfo;
    }

    public async Task<string> BuildScriptAsync(MicroserviceInfo microserviceInfo)
    {
        // Строим дерево вариантов цепочки обязанности
        switch (microserviceInfo.Architecture.Architecture)
        {
            case Architecture.NLayer:
                await BuildAndRunNlayerBuildersAsync(microserviceInfo);
                break;
            case Architecture.Mvc:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return microserviceInfo.Script.ToString();
    }

    private async Task BuildAndRunNlayerBuildersAsync(MicroserviceInfo microserviceInfo)
    {
        var baseUnit = new NlayerBaseBuilder();
        baseUnit.WithSuccessor(new NlayerApiBuilder())
            .WithSuccessor(new NlayerDataBuilder())
            .WithSuccessor(new NlayerOtherLayersBuilder());
        await baseUnit.HandleRequestAsync(microserviceInfo);
    }

    private async Task BuildAndRunNlayerAnalyzersAsync(MicroserviceInfo microserviceInfo)
    {
        // TODO: решить проблему с di.
        var baseUnit = new AnalyzeArchitectureVariationUnit();
        baseUnit.WithSuccessor(new NlayerApiAnalyzer(new ProjectService()))
            .WithSuccessor(new NlayerDataAnalyzer(new ProjectService()))
            .WithSuccessor(new NlayerNameAnalyzer());
        await baseUnit.HandleRequestAsync(microserviceInfo);
    }

    private bool DeleteAnalyzedMicroservice()
    {
        var path = MicroserviceHelper.PathToAnalyzedMicroservice;
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
                Console.WriteLine($"Папка '{path}' успешно удалена");
                return true;
            }
            else
            {
                Console.WriteLine($"Папка '{path}' не существует");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении папки '{path}': {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        //DeleteAnalyzedMicroservice();
    }
}