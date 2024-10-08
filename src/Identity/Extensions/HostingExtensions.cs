using Serilog;

namespace Identity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddRazorPages();

        services.AddCustomIdentity(builder.Configuration);
        services.AddCustomIdentityServer(builder.Configuration);

        services.AddMigration<PersistedGrantDbContext>();
        services.AddMigration<ApplicationDbContext, ApplicationDbContextSeed>();
        services.AddMigration<ConfigurationDbContext, ConfigurationDbContextSeed>();
        
        services.AddCors(options =>
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

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.UseAuthentication();

        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
