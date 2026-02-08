using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERBMS.Infrastructure.Data;

public static class SeedDataExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider services, IConfiguration configuration)
    {
        await SeedData.SeedAsync(services, configuration);
    }
}
