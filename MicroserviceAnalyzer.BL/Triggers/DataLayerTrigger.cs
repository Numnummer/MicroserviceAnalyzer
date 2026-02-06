using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.Triggers;

public class DataLayerTrigger: Trigger
{
    private readonly string[] _dataLayers = ["dal", "dataaccess", "repository", "persistence"];
    public FileSystem.TreeNode? DataLayerNode { get; set; }
    protected override byte CalculateTriggerPercentage()
    {
        if (DataLayerNode == null)
            return 0;
        return _dataLayers.Any(option => DataLayerNode.Name.ToLower().Contains(option))
            ? (byte)100
            : (byte)0;
    }
}