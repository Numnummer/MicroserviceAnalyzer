using MicroserviceAnalyzer.BL;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.DataAccess;

public class MicroserviceInfoStorageItem
{
    public MicroserviceInfo MicroserviceInfo { get; set; }
    public Guid Id { get; set; }
}