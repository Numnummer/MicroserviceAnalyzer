using MicroserviceAnalyzer.BL.Abstractions.Models;
using MicroserviceAnalyzer.BL.Entities;
using MicroserviceAnalyzer.BL.Models;

namespace MicroserviceAnalyzer.BL.BuilderChain.Nlayer;

public class NlayerDataBuilder:ChainUnit
{
    private const string psqlScript = "echo \"Setting up for PostgreSQL with Npgsql\"\ncd $name.Web\n#specific_provider\ndotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.2\ndotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.20\ndotnet add reference ../$name.Infrastructure/$name.Infrastructure.csproj\nsed -i '1s|^|using $name.Infrastructure;\\\\n|' Program.cs\nsed -i '1s|^|using Microsoft.EntityFrameworkCore;\\\\n|' Program.cs\n#specific_provider\nsed -i '/var builder = WebApplication.CreateBuilder(args);/a \\\\nbuilder.Services.AddDbContext<PgsqlDbContext>(options => options.UseNpgsql(builder.Configuration[\\\"PgsqlConnectionStrings:DefaultConnection\\\"]));' Program.cs\n#specific_provider\nAPPSETTINGS=\\\"appsettings.json\\\" && APPSETTINGS_DEV=\\\"appsettings.Development.json\\\" && NEW_CONNECTION=\\\"\\\\\\\"PgsqlConnectionStrings\\\\\\\": {\\\\n    \\\\\\\"DefaultConnection\\\\\\\": \\\\\\\"Host=your_host;Database=your_database;Username=your_username;Password=your_password\\\\\\\"\\\\n  },\\\" && sed -i \\\"/^{/a $NEW_CONNECTION\\\" \\\"$APPSETTINGS\\\" && sed -i \\\"/^{/a $NEW_CONNECTION\\\" \\\"$APPSETTINGS_DEV\\\"\ncd ../$name.Infrastructure\ndotnet add package Microsoft.EntityFrameworkCore --version 8.0.20\ncd DatabaseContext\n#specific_provider\ndotnet new class -n PgsqlDbContext --project ../$name.Infrastructure.csproj\n#specific_provider\nFILE=PgsqlDbContext.cs && sed -i '1i using Microsoft.EntityFrameworkCore;' $FILE && sed -i 's|public class PgsqlDbContext|public class PgsqlDbContext : DbContext|' $FILE && sed -i '/^\\\\s*}\\\\s*$/i \\\\    public PgsqlDbContext(DbContextOptions<PgsqlDbContext> options) : base(options) \\\\    { \\\\    } \\\\' $FILE\ncd ../..\n";
    private const string sqlservScript = "echo \"Pluggable efcore\"\ncd $name.Web\ndotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.20\n#specific_provider\ndotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.20\ndotnet add reference ../$name.Infrastructure/$name.Infrastructure.csproj\nsed -i '1s|^|using $name.Infrastructure;\\\\n|' Program.cs\nsed -i '1s|^|using Microsoft.EntityFrameworkCore;\\\\n|' Program.cs\n#specific_provider\nsed -i '/var builder = WebApplication.CreateBuilder(args);/a \\\\nbuilder.Services.AddDbContext<SqlservDbContext>(options => options.UseSqlServer(builder.Configuration[\\\"SqlservConnectionStrings:DefaultConnection\\\"]));' Program.cs\n#specific_provider\nAPPSETTINGS=\\\"appsettings.json\\\" && APPSETTINGS_DEV=\\\"appsettings.Development.json\\\" && NEW_CONNECTION=\\\"\\\\\\\"SqlservConnectionStrings\\\\\\\": {\\\\n    \\\\\\\"DefaultConnection\\\\\\\": \\\\\\\"Server=your_server;Database=your_database;User Id=your_username;Password=your_password;\\\\\\\"\\\\n  },\\\" && sed -i \\\"/^{/a $NEW_CONNECTION\\\" \\\"$APPSETTINGS\\\" && sed -i \\\"/^{/a $NEW_CONNECTION\\\" \\\"$APPSETTINGS_DEV\\\"\ncd ../$name.Infrastructure\ndotnet add package Microsoft.EntityFrameworkCore --version 8.0.20\ncd DatabaseContext\n#specific_provider\ndotnet new class -n SqlservDbContext --project ../$name.Infrastructure.csproj\n#specific_provider\nFILE=SqlservDbContext.cs && sed -i '1i using Microsoft.EntityFrameworkCore;' $FILE && sed -i 's|public class SqlservDbContext|public class SqlservDbContext : DbContext|' $FILE && sed -i '/^\\\\s*}\\\\s*$/i \\\\    public SqlservDbContext(DbContextOptions<SqlservDbContext> options) : base(options) \\\\    { \\\\    } \\\\' $FILE\ncd ../..\n";
    private const string sqliteScript = "echo \"Setting up for SQLite\"\ncd $name.Web\n#specific_provider\ndotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.20\ndotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.20\ndotnet add reference ../$name.Infrastructure/$name.Infrastructure.csproj\nsed -i '1s|^|using $name.Infrastructure;\\\\n|' Program.cs\nsed -i '1s|^|using Microsoft.EntityFrameworkCore;\\\\n|' Program.cs\n#specific_provider\nsed -i '/var builder = WebApplication.CreateBuilder(args);/a \\\\nbuilder.Services.AddDbContext<SqliteDbContext>(options => options.UseSqlite(builder.Configuration[\\\"SqliteConnectionStrings:DefaultConnection\\\"]));' Program.cs\n#specific_provider\nAPPSETTINGS=\\\"appsettings.json\\\" && APPSETTINGS_DEV=\\\"appsettings.Development.json\\\" && NEW_CONNECTION=\\\"\\\\\\\"SqliteConnectionStrings\\\\\\\": {\\\\n    \\\\\\\"DefaultConnection\\\\\\\": \\\\\\\"Data Source=LocalDatabase.db\\\\\\\"\\\\n  },\\\" && sed -i \\\"/^{/a $NEW_CONNECTION\\\" \\\"$APPSETTINGS\\\" && sed -i \\\"/^{/a $NEW_CONNECTION\\\" \\\"$APPSETTINGS_DEV\\\"\ncd ../$name.Infrastructure\ndotnet add package Microsoft.EntityFrameworkCore --version 8.0.20\ncd DatabaseContext\n#specific_provider\ndotnet new class -n SqliteDbContext --project ../$name.Infrastructure.csproj\n#specific_provider\nFILE=SqliteDbContext.cs && sed -i '1i using Microsoft.EntityFrameworkCore;' $FILE && sed -i 's|public class SqliteDbContext|public class SqliteDbContext : DbContext|' $FILE && sed -i '/^\\\\s*}\\\\s*$/i \\\\    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options) \\\\    { \\\\    } \\\\' $FILE\ncd ../..\n";

    public override async Task HandleRequestAsync(MicroserviceInfo request)
    {
        if (request.DataInfo.HasEfCoreContext)
        {
            request.Script.Append(GetEfcoreScript(request.DataInfo.DotnetVersion, request.DataInfo.EfCoreProvider));
        }
        await NextUnitAsync(request);
    }

    private string GetEfcoreScript(string dotnetVersion, EfCoreProvider? provider)
    {
        const string startEfCoreRegion = "#efcore\n";
        var createDataLayerProjectScripts =
            $"dotnet new classlib --name $name.Infrastructure\ndotnet sln $name.sln add $name.Infrastructure\ncd $name.Infrastructure\nproj_version=\"{dotnetVersion}\"\nfind . -name \"$name.Infrastructure.csproj\" | while read -r file; do\n    if _ -f \"$file\" _; then\n        sed -i \"s|<TargetFramework>.*</TargetFramework>|<TargetFramework>$proj_version</TargetFramework>|g\" \"$file\"\n    fi\ndone\nmkdir DatabaseContext\ncd ..\n";
        var createEfCoreScript = provider!=null ? provider switch
        {
            EfCoreProvider.psql => psqlScript,
            EfCoreProvider.sqlserv => sqlservScript,
            EfCoreProvider.sqlite => sqliteScript,
            _ => psqlScript
        } : psqlScript;
        const string endEfCoreRegion = "#endefcore\n";
        return startEfCoreRegion + createDataLayerProjectScripts 
                                 + createEfCoreScript + endEfCoreRegion;
    }
}