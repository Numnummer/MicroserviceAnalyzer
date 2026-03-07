using MicroserviceAnalyzer.BL.Abstractions.Services;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Models;
using MicroserviceAnalyzer.BL.Models.AnalyzerChain.Nlayer;
using MicroserviceAnalyzer.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace MicroserviceAnalyzer.Tests;

[TestFixture]
public class NlayerDataAnalyzerTests
{
    private Mock<IProjectService> _projectServiceMock;
    private NlayerDataAnalyzer _analyzer;

    [SetUp]
    public void Setup()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _analyzer = new NlayerDataAnalyzer(_projectServiceMock.Object);
    }

    [Test]
    public async Task HasEfCorePredictTrue()
    {
        var request = new MicroserviceInfo
        {
            FileSystem = new FileSystem(TestMicroservicesHelper.GetMicroservicePath("NlayerWithEfCore"))
        };
        await _analyzer.HandleRequestAsync(request);
        Assert.That(request.DataInfo.HasEfCoreContext);
    }

    [Test]
    public async Task HasNotEfCorePredictTrue()
    {
        var request = new MicroserviceInfo
        {
            FileSystem = new FileSystem(TestMicroservicesHelper.GetMicroservicePath("NlayerWithoutEfCore"))
        };
        await _analyzer.HandleRequestAsync(request);
        Assert.That(!request.DataInfo.HasEfCoreContext);
    }
    
    [Test]
    public async Task HasSqlservProvider()
    {
        var request = new MicroserviceInfo
        {
            FileSystem = new FileSystem(TestMicroservicesHelper.GetMicroservicePath("NlayerWithEfCore"))
        };
        await _analyzer.HandleRequestAsync(request);
        Assert.That(request.DataInfo.EfCoreProvider, Is.EqualTo(EfCoreProvider.sqlserv));
    }
}