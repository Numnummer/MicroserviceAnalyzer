using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.BuilderChain.Nlayer;

public class NlayerApiBuilder:ChainUnit
{
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        if (request.ApiInfo.HasWeb)
        {
            request.Script.Append(BuildWebScript(request.ApiInfo.DotnetVersion));
        }
        if (request.ApiInfo.HasGrpc)
        {
            request.Script.Append(BuildGrpcScript(request.ApiInfo.DotnetVersion));
        }
        await NextUnitAsync(request);
    }

    private string BuildWebScript(string projectVersion)
    {
        return
            $"dotnet new web --name $name.Web\ndotnet sln $name.sln add $name.Web\ncd $name.Web\nproj_version=\"{projectVersion}\"\nfind . -name \"$name.Web.csproj\" | while read -r file; do\n    if _ -f \"$file\" _; then\n        sed -i \"s|<TargetFramework>.*</TargetFramework>|<TargetFramework>$proj_version</TargetFramework>|g\" \"$file\"\n    fi\ndone\ncd ..\n";
    }

    private string BuildGrpcScript(string projectVersion)
        => $"dotnet new grpc --name $name.Web\ndotnet sln $name.sln add $name.Web\ncd $name.Web\nproj_version=\"{projectVersion}\"\nfind . -name \"$name.Web.csproj\" | while read -r file; do\n    if _ -f \"$file\" _; then\n        sed -i \"s|<TargetFramework>.*</TargetFramework>|<TargetFramework>$proj_version</TargetFramework>|g\" \"$file\"\n    fi\ndone\ncd ..\n";
}