namespace Identity;

public class IdentityOptions : DbContextOptions
{
    private const int _minimalLength = 8;
    private bool _requireDigit = true;
    private bool _requireLowercase = true;
    private bool _requireUppercase = true;
    private int _requiredLength = _minimalLength;
    private bool _requireNonAlphanumeric = true;

    public bool RequireDigit
    {
        get => _requireDigit;
        set => _requireDigit = value;
    }

    public bool RequireLowercase
    {
        get => _requireLowercase;
        set => _requireLowercase = value;
    }

    public bool RequireUppercase
    {
        get => _requireUppercase;
        set => _requireUppercase = value;
    }

    public int RequiredLength
    {
        get => _requiredLength;
        set
        {
            if (value < _minimalLength)
                throw new ArgumentOutOfRangeException(nameof(value), $"Required length must be no less then {_minimalLength} number.");
            _requiredLength = value;
        }
    }

    public bool RequireNonAlphanumeric
    {
        get => _requireNonAlphanumeric;
        set => _requireNonAlphanumeric = value;
    }
}
