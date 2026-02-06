namespace MicroserviceAnalyzer.Tests.Helpers;

public static class TestMicroservicesHelper
{
    public static string GetMicroservicePath(string name)
    {
        return Path.Combine(GetCurrentSolutionPath(), "TestMicroservices", name);
    }
    private static string GetCurrentSolutionPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? throw new NullReferenceException("Unable to get current solution path");
    }
}