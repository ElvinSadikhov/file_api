using System.Reflection;
using AtomCore.JWT;
using AtomCore.Validation;
using AttributeInjection.Markers;
using Core.Logging;
using FluentValidation;
using MediatR;

namespace Application;

public class ApplicationAR : AssemblyRegistrator
{
    public override void AddDependenciesManually(IServiceCollection services)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        services.AddMediatR(c => c.RegisterServicesFromAssembly(executingAssembly));

        services.AddJwtHelper();

        // services.Configure<AccessTokenOption>(config.GetSection("Jwt:Access"));
        // services.Configure<RefreshTokenOption>(config.GetSection("Jwt:Refresh"));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipeline<,>));
        services.AddTokenCheckPipeline("Jwt:Access");
        // services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UserIsNotDeletedTokenValidation<,>));
        // services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TokenVersionValidation<,>));
        // services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UserRoleAuthorizationPipeline<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));
        
        // services.Decorate<IFilePersister, FilePersisterCacheDecorator>();

        services.AddValidatorsFromAssembly(executingAssembly);
        services.AddAutoMapper(_ => { }, executingAssembly);
        services.AddHttpContextAccessor();
        services.AddHttpClient();
    }
}