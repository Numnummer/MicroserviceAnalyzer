using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Helpers;
using MicroserviceAnalyzer.BL.Triggers;
using MicroserviceAnalyzer.BL.Variations;

namespace MicroserviceAnalyzer.BL.Models.AnalyzerChain;

[Obsolete("Эта обработка больше не нужна")]
public class AnalyzeArchitectureVariationUnit:ChainUnit
{
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        switch (request.Architecture.Architecture)
        {
            case Architecture.NLayer:
                ProcessNLayer(request);
                break;
            case Architecture.Mvc:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await NextUnitAsync(request);
    }

    private void ProcessNLayer(MicroserviceInfo request)
    {
        var folders= request?.FileSystem?.TraverseDfs()
            .Where(node => node.IsDirectory)
            .Select(node=>node.Name)
            .ToArray();
        var mvcTrigger = new MvcTrigger
        {
            FolderNames = folders
        };
        request?.Architecture.ArchitectureVariations.Add(NlayerVariation.Standard.ToString());
        if (mvcTrigger.GetTriggerPercentage() != 100) return;
        request?.Keywords.AddRange(Keywords.MvcKeywords);
        request?.Architecture.ArchitectureVariations.Remove(NlayerVariation.Standard.ToString());
        request?.Architecture.ArchitectureVariations.Add(NlayerVariation.Mvc.ToString());
    }
}