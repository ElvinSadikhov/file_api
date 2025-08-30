using Application;
using AtomCore.ExceptionHandling.RestAPIHandler;
using AttributeInjection.Extensions;
using Core;
using Persistence;
using RestAPI.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAttributeInjection(new ApplicationAR(), new PersistenceAR(builder.Configuration), new CoreAR());
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddGlobalExceptionJsonResponseCreator();

var app = builder.Build();

app.UseMiddleware<LoggingMiddleware>();
app.UseGlobalExceptionHandler();
app.MapScalarApiReference();
app.MapControllers();
app.MapOpenApi();
app.Run();

// todo: transfer to readme
// 1) dotnet publish -c Release -o ./publish
// 2) cd publish
// 3) export ASPNETCORE_ENVIRONMENT=Development
//          OR
//    export ASPNETCORE_ENVIRONMENT=Production
// 4) dotnet RestAPI.dll