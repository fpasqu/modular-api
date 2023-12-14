using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;
using System.Reflection;
using ModularApi.DependencyInjection;
using ModularApi.Models.Data;
using OpenTelemetry.Metrics;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MyServiceCollection
    {
        public static void AddAppServices(WebApplicationBuilder builder)
        {
            string dbtype = builder.Configuration.GetSection("DbType").Value;
            switch (dbtype) 
            {
                case "I":
                    builder.Services.AddScoped<ITodoRepository, InMemoryRepository>();
                    break;
                case "S":
                    builder.Services.AddDbContext<TodoDbContext>();
                    builder.Services.AddScoped<ITodoRepository, TodoRepository>();
                    break;
                case "C":
                    builder.Services.AddSingleton<ITodoRepository>(PrepareCosmosDb.CosmosService(builder.Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());
                    break;
                default:
                    break;
            }

            //di
            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

            //seq
            builder.Host.UseSerilog((ctx, loggercfg) => 
            {
                loggercfg.ReadFrom.Configuration(ctx.Configuration)
                .WriteTo.Console()
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .Enrich.With<ActivityEnricher>()
                .WriteTo.Seq("http://localhost:5341");
            });

            //error handling
            builder.Services.AddProblemDetails(opt => 
            {
                //cuts out most of the internal code from errors
                opt.IncludeExceptionDetails = (httpctx, ex) => false;
                opt.OnBeforeWriteDetails = (httpctx, details) =>
                {
                    //instead of the source code, we provide a custom message
                    if (details.Status == 500)
                    {
                        details.Detail = "Something went wrong. Use the following traceId for support";
                    }
                };
            });

            //req/res logging
            builder.Services.AddHttpLogging(opt =>
            {
                opt.LoggingFields = HttpLoggingFields.All;
                opt.MediaTypeOptions.AddText("application/javascript");
                opt.RequestBodyLogLimit = 4096;
                opt.ResponseBodyLogLimit = 4096;
            });

            //health check
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<TodoDbContext>();

            var appName = builder.Environment.ApplicationName;
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(builder => builder.AddService(serviceName: appName))
                .WithTracing(bld => bld.AddAspNetCoreInstrumentation().AddOtlpExporter())
                .WithMetrics(bld => bld.AddAspNetCoreInstrumentation().AddOtlpExporter());

            ////telemetry
            //builder.Services.AddOpenTelemetry(bldr =>
            //{
            //    bldr.SetResourceBuilder(ResourceBuilder.CreateDefault()
            //    .AddService(builder.Environment.ApplicationName))
            //    .AddAspNetCoreInstrumentation()
            //    .AddEntityFrameworkCoreInstrumentation()
            //    .AddOtlpExporter(opt => { opt.Endpoint = new Uri("http://localhost:4317"); });
            //});

            //logger
            //generating and writing to the path
            //var path = Path.Join(Environment.CurrentDirectory, "custom-logs");
            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}
            //var tracePath = Path.Join(path, $"Log_TodoApiSqlserver_{DateTime.Now.ToString("yyyyMMdd-HHmm")}.txt");
            //Trace.Listeners.Add(new TextWriterTraceListener(File.CreateText(tracePath)));
            //Trace.AutoFlush = true;
        }
    }
}
