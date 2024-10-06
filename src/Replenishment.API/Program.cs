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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try 
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    builder.Configuration.AddEnvironmentVariables();

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();

    using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        var client = scope.ServiceProvider.GetRequiredService<IStreamingRpcClient>();
        await client.ConnectAsync();
    }

    app.Run();
} 
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}