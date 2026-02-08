using Microsoft.Extensions.Configuration;
using Serilog;

namespace ERBMS.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static LoggerConfiguration Configure(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        return loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext();
    }
}
