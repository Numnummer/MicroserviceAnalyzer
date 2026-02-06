using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.Triggers;

public class OtherLayersTrigger:Trigger
{
    public readonly string[] OtherLayers = ["application"];
    public FileSystem.TreeNode? OtherLayerNode { get; set; }
    protected override byte CalculateTriggerPercentage()
    {
        if (OtherLayerNode == null)
            return 0;
        return OtherLayers.Any(option => OtherLayerNode.Name.ToLower().Contains(option))
            ? (byte)100
            : (byte)0;
    }
}