using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.BuilderChain.Nlayer;

public class NlayerDataBuilder:ChainUnit
{
    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        if (request.DataInfo.HasEfCoreContext)
        {
            request.Script.Append(GetEfcoreScript(request.DataInfo.DotnetVersion));
        }
        await NextUnitAsync(request);
    }

    private string GetEfcoreScript(string dotnetVersion)
    {
        const string startEfCoreRegion = "#efcore\n";
        var createDataLayerProjectScripts =
            $"dotnet new classlib --name $name.DataAccess\ndotnet sln $name.sln add $name.DataAccess\ncd $name.DataAccess\nproj_version=\"{dotnetVersion}\"\nfind . -name \"$name.DataAccess.csproj\" | while read -r file; do\n    if _ -f \"$file\" _; then\n        sed -i \"s|<TargetFramework>.*</TargetFramework>|<TargetFramework>$proj_version</TargetFramework>|g\" \"$file\"\n    fi\ndone\nmkdir DatabaseContext\ncd ..\n";
        const string createEfCoreScript = "echo \"Setting up for PostgreSQL with Npgsql\"\ncd $name.Web\n# Add the PostgreSQL provider package instead of SQL Server\ndotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.2\ndotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.20\ndotnet add reference ../$name.DataAccess/$name.DataAccess.csproj\nsed -i '1s|^|using $name.DataAccess;\\n|' Program.cs\nsed -i '1s|^|using Microsoft.EntityFrameworkCore;\\n|' Program.cs\nsed -i '/var builder = WebApplication.CreateBuilder(args);/a \\\nbuilder.Services.AddDbContext<PgsqlDbContext>(options => options.UseNpgsql(builder.Configuration[\"PgsqlConnectionStrings:DefaultConnection\"]));' Program.cs\nAPPSETTINGS=\"appsettings.json\"\nAPPSETTINGS_DEV=\"appsettings.Development.json\"\nNEW_CONNECTION=\"\\\"PgsqlConnectionStrings\\\": {{\\n    \\\"DefaultConnection\\\": \\\"Host=your_host;Database=your_database;Username=your_username;Password=your_password\\\"\\n  }},\"\nsed -i \"/^{{/a $NEW_CONNECTION\" \"$APPSETTINGS\"\nsed -i \"/^{{/a $NEW_CONNECTION\" \"$APPSETTINGS_DEV\"\ncd ../$name.DataAccess\ndotnet add package Microsoft.EntityFrameworkCore --version 8.0.20\ncd DatabaseContext\ndotnet new class -n PgsqlDbContext --project ../$name.DataAccess.csproj\nFILE=\"PgsqlDbContext.cs\"\n\nsed -i '1s|^|using Microsoft.EntityFrameworkCore;\\n|' \"$FILE\"\n\nsed -i 's|public class PgsqlDbContext|public class PgsqlDbContext : DbContext|' \"$FILE\"\n\nsed -i '/^\\s*}}\\s*$/i \\\n    public PgsqlDbContext(DbContextOptions<PgsqlDbContext> options) : base(options) \\\n    {{ \\\n    }} \\\n' \"$FILE\"\ncd ../..";
        const string endEfCoreRegion = "\n#endefcore\n";
        return startEfCoreRegion + createDataLayerProjectScripts 
                                 + createEfCoreScript + endEfCoreRegion;
    }
}