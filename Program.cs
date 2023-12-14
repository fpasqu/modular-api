using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;
using ModularApi.Middleware;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

//custom services
MyServiceCollection.AddAppServices(builder);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseProblemDetails();
app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

//scope middleware
app.UseMiddleware<UserScopeMiddleware>();

app.UseAuthorization();
app.MapControllers();
app.UseRouting();

//health checks
app.MapHealthChecks("app-health");

app.Run();
