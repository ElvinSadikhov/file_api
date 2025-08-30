using AttributeInjection.Markers;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class PersistenceAR(IConfiguration configuration) : AssemblyRegistrator
{
    public override void AddDependenciesManually(IServiceCollection services)
    {
        // string connectionString = configuration["Postgres:ConnectionString"]!;
        // services.AddDbContext<DatabaseContext>(opt => opt.UseNpgsql(connectionString));
    }
}