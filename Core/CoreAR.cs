using System.Reflection;
using AttributeInjection.Markers;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public class CoreAR : AssemblyRegistrator
{
    public override void AddDependenciesManually(IServiceCollection services)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(config => config.RegisterServicesFromAssembly(executingAssembly));
    }
}