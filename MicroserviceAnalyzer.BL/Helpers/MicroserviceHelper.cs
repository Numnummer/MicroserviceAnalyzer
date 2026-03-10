namespace MicroserviceAnalyzer.BL.Helpers;

public static class MicroserviceHelper
{
    public static readonly string PathToAnalyzedMicroservice = Path.Combine(
        "/tmp",
        "TemplateMicroservice"
    );
    public static string[] GetPathComponentsRelativeToMicroservice(string path)
        => path.Split(Path.DirectorySeparatorChar)[4..];
}