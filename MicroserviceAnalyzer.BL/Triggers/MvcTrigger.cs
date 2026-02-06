namespace MicroserviceAnalyzer.BL.Triggers;

public class MvcTrigger:Trigger
{
    public string[]? FolderNames { get; set; }
    protected override byte CalculateTriggerPercentage()
    {
        if (FolderNames == null || FolderNames.Length == 0)
            return 0;
        var normalizedFolders = FolderNames.Select(f => f.ToLowerInvariant()).ToArray();
        var totalScore = 0.0;
        var hasMvc = normalizedFolders.Any(f=>f is "model" or "models")
            && normalizedFolders.Any(f=>f is "view" or "views")
            && normalizedFolders.Any(f=>f is "controller" or "controllers");
        totalScore = hasMvc? 100 : 0;
        
        return (byte)totalScore;
    }
}