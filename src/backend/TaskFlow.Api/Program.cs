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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();