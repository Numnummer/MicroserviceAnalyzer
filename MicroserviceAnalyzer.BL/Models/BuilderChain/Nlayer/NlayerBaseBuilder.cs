using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.BuilderChain.Nlayer;

public class NlayerBaseBuilder:ChainUnit
{
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        var baseScript = $"#arch_NLayer\nrm -r src\nmkdir src\ncd src\nname={request.Name}\ndotnet new sln --name $name\n";
        request.Script.Append(baseScript);
        await NextUnitAsync(request);
    }
}