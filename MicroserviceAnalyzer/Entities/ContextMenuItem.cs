using System;

namespace MicroserviceAnalyzer.Entities;

public class ContextMenuItem
{
    public string? Name { get; set; }
    public Action Handler { get; set; } = () => { };
}
