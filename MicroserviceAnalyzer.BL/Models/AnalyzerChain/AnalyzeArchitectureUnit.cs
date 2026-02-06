using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Helpers;
using MicroserviceAnalyzer.BL.Triggers;
using MicroserviceAnalyzer.BL.Variations;

namespace MicroserviceAnalyzer.BL.Models.AnalyzerChain;

public class AnalyzeArchitectureUnit:ChainUnit
{
    /// <summary>
    /// Анализ архитектуры микросервиса.
    /// Переход на следующего обработчика в цепочке
    /// происходит вручную в зависимости от вычисленной архитектуры.
    /// Результат записывается в переданный параметр.
    /// </summary>
    /// <param name="request"></param>
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        await Task.Run(() =>
        {
            ProcessTriggers(request);
        });
    }

    private void ProcessTriggers(MicroserviceInfo request)
    {
        var fileSystem = new FileSystem(MicroserviceHelper.PathToAnalyzedMicroservice);
        var folders= fileSystem.TraverseDfs()
            .Where(node => node.IsDirectory)
            .Select(node=>node.Name)
            .ToArray();
        var nlayerTrigger = new NLayerTrigger
        {
            FolderNames = folders
        };
        var mvcTrigger = new MvcTrigger
        {
            FolderNames = folders
        };
        var nlayerResult = nlayerTrigger.GetTriggerPercentage();
        var mvcResult = mvcTrigger.GetTriggerPercentage();
        if (nlayerResult > mvcResult)
        {
            request.Architecture.Architecture = Architecture.NLayer;
            request.Keywords.AddRange(Keywords.MvcKeywords);
        }
        else
        {
            request.Architecture.Architecture = Architecture.Mvc;
            request.Keywords.AddRange(Keywords.NlayerKeywords);
        }
        request.FileSystem = fileSystem;
    }
    
}