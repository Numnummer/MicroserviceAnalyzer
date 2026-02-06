using MicroserviceAnalyzer.BL.Abstractions.Services;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Models;
using MicroserviceAnalyzer.BL.Models.AnalyzerChain.Nlayer;
using MicroserviceAnalyzer.Tests.Helpers;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace MicroserviceAnalyzer.Tests;

[TestFixture]
public class NlayerApiAnalyzerTests
{
    private Mock<IProjectService> _projectServiceMock;
    private NlayerApiAnalyzer _analyzer;
    
    [SetUp]
    public void Setup()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _analyzer = new NlayerApiAnalyzer(_projectServiceMock.Object);
    }

    [Test]
    public async Task HasApiWebLayerPredictsTrue()
    {
        var request = new MicroserviceInfo
        {
            FileSystem = new FileSystem(TestMicroservicesHelper.GetMicroservicePath("NlayerWithApiWeb"))
        };
        await _analyzer.HandleRequestAsync(request);
        
        Assert.That(request.ApiInfo.HasWeb);
        Assert.That(!request.ApiInfo.HasGrpc);
    }
    
    [Test]
    public async Task HasApiGrpcLayerPredictsTrue()
    {
        var request = new MicroserviceInfo
        {
            FileSystem = new FileSystem(TestMicroservicesHelper.GetMicroservicePath("NLayerWithApiGrpc"))
        };
        await _analyzer.HandleRequestAsync(request);
        
        Assert.That(request.ApiInfo.HasGrpc);
        Assert.That(!request.ApiInfo.HasWeb);
    }
}