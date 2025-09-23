using System.Reflection;
using AtomCore.Validation;
using AttributeInjection.Markers;
using Core.Logging;
using FluentValidation;
using MediatR;
using StackExchange.Redis;

namespace Application;

public class ApplicationAR : AssemblyRegistrator
{
    public override void AddDependenciesManually(IServiceCollection services)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        // var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        services.AddMediatR(c => c.RegisterServicesFromAssembly(executingAssembly));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipeline<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));

        // services.Decorate<IFilePersister, FilePersisterCacheDecorator>();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")!));

        services.AddValidatorsFromAssembly(executingAssembly);
        services.AddAutoMapper(_ => { }, executingAssembly);
        services.AddHttpContextAccessor();
        services.AddHttpClient();
    }
}