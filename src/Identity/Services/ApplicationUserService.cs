using System.Security.Claims;

namespace Identity.Services;

public class ApplicationUserService : ProfileService<ApplicationUser>
{
    public ApplicationUserService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory) : base(userManager, claimsFactory)
    {
    }

    protected override async Task GetProfileDataAsync(ProfileDataRequestContext context, ApplicationUser user)
    {
        var principal = await GetUserClaimsAsync(user);
        var id = (ClaimsIdentity)principal.Identity!;
        if (!string.IsNullOrEmpty(user.FavoriteColor))
        {
            id.AddClaim(new Claim("favorite_color", user.FavoriteColor));
        }

        context.AddRequestedClaims(principal.Claims);
    }
}