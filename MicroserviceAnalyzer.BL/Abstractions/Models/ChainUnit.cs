using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.Abstractions.Models;

/// <summary>
/// База обработчика микросервиса.
/// </summary>
public abstract class ChainUnit
{
    protected ChainUnit? Successor;
    public ChainUnit WithSuccessor(ChainUnit successor)
    {
        Successor = successor;
        return successor;
    }
    public abstract Task HandleRequestAsync(MicroserviceInfo request);

    protected async Task NextUnitAsync(MicroserviceInfo request)
    {
        if(Successor != null)
            await Successor.HandleRequestAsync(request);
    }
}