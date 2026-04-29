using TaskFlow.Api;
using TaskFlow.Application;
using TaskFlow.Domain;
using TaskFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiLayerServices()
    .AddInfrastructureLayerServices(builder.Configuration)
    .AddApplicationLayerServices()
    .AddDomainLayerServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "TaskFlow.Api v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();