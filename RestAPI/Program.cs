using System.Text.RegularExpressions;
using Amazon.S3.Model;
using Application;
using Application.Jobs;
using AtomCore.ExceptionHandling.RestAPIHandler;
using AtomCore.Extensions;
using AttributeInjection.Extensions;
using Core;
using Persistence;
using Quartz;
using RestAPI.Middlewares;
using Scalar.AspNetCore;

// todo: fix atomicity problems EVERYWHERE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
EnsureEnvVarsAreSet();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAttributeInjection(new ApplicationAR(), new PersistenceAR(builder.Configuration), new CoreAR());
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddGlobalExceptionJsonResponseCreator();
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ClearUnfinishedUploadRecordsJob");
    q.AddJob<ClearUnfinishedUploadRecordsJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ClearUnfinishedUploadRecordsJob-trigger")
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(60).RepeatForever()));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


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

void EnsureEnvVarsAreSet()
{
    List<EnvChecker> envCheckers = new()
    {
        new()
        {
            Key = "REDIS_CONNECTION_STRING",
            RegExp = @"^([a-zA-Z0-9\.\-]+):(\d+),user=([^,]+),password=(.+)$",
        },
        new()
        {
            Key = "AWS_ACCESS_KEY_ID"
        },
        new()
        {
            Key = "AWS_SECRET_ACCESS_KEY"
        },
        new()
        {
            Key = "AWS_S3_BUCKET_NAME"
        },
        // new()
        // {
        //     Key = "AWS_S3_REGION_SYSTEM_NAME"
        // },
        new()
        {
            Key = "MAX_ALLOWED_PART_SIZE_IN_BYTES",
            RegExp = @"^[1-9]\d*$",
        },
    };

    foreach (var checker in envCheckers)
    {
        if (!checker.Check())
        {
            throw new Exception(
                $"Environment variable {checker.Key} is not set or has invalid format. Regex: {checker.RegExp ?? "N/A"}");
        }
    }
}

class EnvChecker
{
    public string Key { get; set; }
    public string? RegExp { get; set; }

    public bool Check()
    {
        var envVar = Environment.GetEnvironmentVariable(Key);

        if (envVar is null || envVar.Trim().IsNullOrEmpty()) return false;

        if (RegExp is null) return true;

        return Regex.IsMatch(envVar, RegExp);
    }
}