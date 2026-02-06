namespace MicroserviceAnalyzer.BL.Helpers;

public static class Keywords
{
    public static readonly string[] MvcKeywords = ["model", "view", "controller"];
    public static readonly string[] NlayerKeywords = ["api", "presentation",
        "BL", "Logic", "BusinessLogic", "Domain", "Core", "Service",
        "DAL", "DataAccess", "Repository", "Persistence"];
    public static readonly string[] NlayerCleanKeywords = ["usecases"];
    public static readonly string[] GrpcKeywords = ["grpc", "proto"];
}