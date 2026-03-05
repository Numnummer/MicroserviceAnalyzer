using System;

namespace MicroserviceAnalyzer.Entities;

public class FileTextData
{
    public string? FileName { get; set; }
    public string? FileRelativeToMicroservicePath { get; set; }
    public string? Text { get; set; }
}
