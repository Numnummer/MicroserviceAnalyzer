namespace MicroserviceAnalyzer.BL.Triggers;

public class NLayerTrigger: Trigger
{
    private readonly string[] _apiLayers = ["api", "presentation"];
    private readonly string[] _coreLayers = ["BL", "Logic", "BusinessLogic", "Domain", "Core", "Service"];
    private readonly string[] _dataLayers = ["DAL", "DataAccess", "Repository", "Persistence"];
    public string[]? FolderNames { get; set; }
    protected override byte CalculateTriggerPercentage()
    {
        if (FolderNames == null || FolderNames.Length == 0)
            return 0;

        var normalizedFolders = FolderNames.Select(f => f.ToLowerInvariant()).ToArray();
        var totalScore = 0.0;

        // Весовые коэффициенты для каждого слоя
        const double presentationWeight = 0.30;  // 30%
        const double businessWeight = 0.30;     // 30%
        const double dataWeight = 0.30;         // 30%
        const double domainWeight = 0.10;       // 10% (опционально)

        // Проверяем каждый слой
        var hasPresentation = normalizedFolders.Any(folder => 
            _apiLayers.Any(api => folder.Contains(api.ToLowerInvariant())));
        
        var hasBusiness = normalizedFolders.Any(folder => 
            _coreLayers.Any(core => folder.Contains(core.ToLowerInvariant())));
        
        var hasData = normalizedFolders.Any(folder => 
            _dataLayers.Any(data => folder.Contains(data.ToLowerInvariant())));
        
        var hasDomain = normalizedFolders.Any(folder => 
            folder.Contains("domain") || folder.Contains("entity") || folder.Contains("model"));

        // Считаем взвешенную сумму
        if (hasPresentation) totalScore += presentationWeight * 100;
        if (hasBusiness) totalScore += businessWeight * 100;
        if (hasData) totalScore += dataWeight * 100;
        if (hasDomain) totalScore += domainWeight * 100;

        return (byte)totalScore;
    }
}