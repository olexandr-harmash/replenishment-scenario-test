using Log = Serilog.Log;

namespace PantsuTapPlayground.Replenishment.Api;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        Log.Information("Начало конфигурации сервисов.");

        services.AddMemoryCache();
        
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ITransferService, TransferService>();

        var clusterConfig = builder.Configuration["Solnet:Cluster"];
        if (string.IsNullOrEmpty(clusterConfig))
        {
            throw new InvalidOperationException("Solnet cluster configuration is missing.");
        }

        var cluster = clusterConfig == "dev" ? Cluster.DevNet : Cluster.MainNet;

        var solnetStreamRpcClient = ClientFactory.GetStreamingClient(cluster);
        services.AddSingleton(solnetStreamRpcClient);

        var solnetRpcClient = ClientFactory.GetClient(cluster);
        services.AddSingleton(solnetRpcClient);


        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(Program));
        });

        services.AddApiVersioning();
  
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        var identityUri = builder.Configuration["Authentication:Uri"];
        if (string.IsNullOrEmpty(identityUri))
        {
            throw new InvalidOperationException("Authentication uri configuration is missing.");
        }

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = identityUri;
                options.TokenValidationParameters.ValidateAudience = false;
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "api1");
            });
        });

        Log.Information("Конфигурация сервисов завершена.");

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        Log.Information("Начало конфигурации pipeline.");

        app.UseCors("AllowAll");

        app.UseHttpsRedirection();

        app.NewVersionedApi("Replenishment")
            .RequireAuthorization("ApiScope")
            .MapReplenishmentApiV1();
    
        Log.Information("Конфигурация pipeline завершена.");

        return app;
    }
}
