namespace Identity.Models;

public class ApplicationUser : IdentityUser
{
    public string? FavoriteColor { get; set; }
}