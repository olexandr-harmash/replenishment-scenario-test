using PantsuTapPlayground.Replenishment.Api.Apis;
using PantsuTapPlayground.Replenishment.Api.Services;
using Solnet.Rpc;

// Project: PantsuTapPlayground.Replenishment.Api
// Author: [Oleksandr Harmash]
// Description: This is the main entry point for the Replenishment API of the Pantsu Tap project. 
//              It sets up and configures the ASP.NET Core application, including middleware, 
//              Swagger documentation, and API versioning for the Replenishment service.
// 
// Documentation: 
// - For more information on configuring Swagger/OpenAPI, visit: https://aka.ms/aspnetcore/swashbuckle
// - For API versioning documentation, refer to: https://olexandrharmash.atlassian.net/wiki/spaces/PT2/pages/12255233/Replenishment+Service.
// - To learn more about CORS and HTTPS setup, check the ASP.NET Core documentation.
// - For more information about service, refer to: https://olexandrharmash.atlassian.net/wiki/spaces/PT2/pages/12255233/Replenishment+Service.
// - Confluence page: [Replenishment Service Details](https://olexandrharmash.atlassian.net/wiki/spaces/PT2/pages/12255233/Replenishment+Service)
// - Jira task: [PT-1](https://olexandrharmash.atlassian.net/browse/PT-1)
// 
// Changes:
// - [Дата и краткое описание изменений]
// 
// TODO: 
// - Setup CORS policy to restrict access to authorized domains.
// - Ensure HTTPS is enforced for secure communication.

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<WalletService>();
builder.Services.AddScoped<CacheService>();
builder.Services.AddSingleton(ClientFactory.GetStreamingClient(Cluster.DevNet));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining(typeof(Program));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning();

 builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<IStreamingRpcClient>();
    await client.ConnectAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.NewVersionedApi("Replenishment")
   .MapReplenishmentApiV1();

app.Run();