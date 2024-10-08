using Serilog;
using Microsoft.EntityFrameworkCore;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.EntityFramework.DbContexts;

namespace Identity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        var migrationsAssembly = typeof(Program).Assembly.GetName().Name;
        var connectionString = builder.Configuration["Storage:Connection"];
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Storage connection configuration is missing.");
        }

        builder.Services.AddIdentityServer(options =>
        {
            // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
            options.EmitStaticAudienceClaim = true;
        })
        .AddConfigurationStore(options =>
        {
            options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));
        })
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = b => b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));
        })
        .AddTestUsers(TestUsers.Users);
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", corsBuilder => corsBuilder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
        app.UseCors("AllowAll");
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        InitializeDatabase(app);

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages().RequireAuthorization();

        return app;
    }

    private static void InitializeDatabase(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices
            .GetService<IServiceScopeFactory>()!
            .CreateScope();

        var persistedGrantDbContext = serviceScope
            .ServiceProvider
            .GetRequiredService<PersistedGrantDbContext>();
        persistedGrantDbContext.Database.Migrate();

        var configDbContext = serviceScope
            .ServiceProvider
            .GetRequiredService<ConfigurationDbContext>();
        configDbContext.Database.Migrate();

        if (!configDbContext.Clients.Any())
        {
            foreach (var client in Config.Clients)
            {
                configDbContext.Clients.Add(client.ToEntity());
            }
            configDbContext.SaveChanges();
        }

        if (!configDbContext.IdentityResources.Any())
        {
            foreach (var resource in Config.IdentityResources)
            {
                configDbContext.IdentityResources.Add(resource.ToEntity());
            }
            configDbContext.SaveChanges();
        }

        if (!configDbContext.ApiScopes.Any())
        {
            foreach (var scope in Config.ApiScopes)
            {
                configDbContext.ApiScopes.Add(scope.ToEntity());
            }
            configDbContext.SaveChanges();
        }
    }
}
