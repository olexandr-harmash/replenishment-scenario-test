using Microsoft.EntityFrameworkCore;
using Serilog;
using Identity.Models;
using Identity.Services;

namespace Identity;

public static class IdentityServerExtensions
{
    public static IServiceCollection AddCustomIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOptions = new IdentityServerOptions();

        try
        {
            configuration.GetSection("IdentityServer").Bind(identityOptions);
            Log.Information("Configuring IdentityServer with connection string: {ConnectionString}", identityOptions.ConnectionString);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in IdentityServer configuration");
            throw;
        }

        var isb = services.AddIdentityServer(options =>
        {
            options.EmitStaticAudienceClaim = true;
            Log.Information("IdentityServer options configured: EmitStaticAudienceClaim = {EmitStaticAudienceClaim}", options.EmitStaticAudienceClaim);
        })
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<ApplicationUserService>();

        isb.AddConfigurationStore(options =>
        {
            Log.Information("Configuring ConfigurationStore with database context.");
            options.ConfigureDbContext = builder =>
                builder.UseNpgsql(identityOptions.ConnectionString, sql => sql.MigrationsAssembly(identityOptions.MigrationsAssembly));
        });

        isb.AddOperationalStore(options =>
        {
            Log.Information("Configuring OperationalStore with database context.");
            options.ConfigureDbContext = builder =>
                builder.UseNpgsql(identityOptions.ConnectionString, sql => sql.MigrationsAssembly(identityOptions.MigrationsAssembly));
        });
           
        Log.Information("IdentityServer successfully configured.");
        return services;
    }
}
