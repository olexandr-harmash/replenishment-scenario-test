using Identity.Infrastructure;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Serilog;

using IdentityOptions = Identity.IdentityOptions;

public static class IdentityExtensions
{
    public static IServiceCollection AddCustomIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOptions = new IdentityOptions();

        try
        {
            configuration.GetSection("Identity").Bind(identityOptions);
            Log.Information("Identity configuration validated successfully.");
        }
        catch (InvalidOperationException ex)
        {
            Log.Error("Error in Identity configuration: {Message}", ex.Message);
            throw;
        }

        Log.Information("Configuring Identity with connection string: {DefaultConnection}", identityOptions.ConnectionString);

        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseNpgsql(identityOptions.ConnectionString, sql => sql.MigrationsAssembly(identityOptions.MigrationsAssembly)));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = identityOptions.RequireDigit;
            options.Password.RequiredLength = identityOptions.RequiredLength;
            options.Password.RequireLowercase = identityOptions.RequireLowercase;
            options.Password.RequireUppercase = identityOptions.RequireUppercase;
            options.Password.RequireNonAlphanumeric = identityOptions.RequireNonAlphanumeric;

            Log.Information("Identity options configured: RequireDigit = {RequireDigit}, RequireLowercase = {RequireLowercase}, RequireUppercase = {RequireUppercase}, RequiredLength = {RequiredLength}, RequireNonAlphanumeric = {RequireNonAlphanumeric}", 
                identityOptions.RequireDigit, 
                identityOptions.RequireLowercase, 
                identityOptions.RequireUppercase, 
                identityOptions.RequiredLength,
                identityOptions.RequireNonAlphanumeric);
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        Log.Information("ASP.NET Core Identity successfully configured.");
        return services;
    }
}
