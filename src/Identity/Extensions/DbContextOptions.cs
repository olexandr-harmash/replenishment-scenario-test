namespace Identity;

public class DbContextOptions
{
    private string _connectionString = string.Empty;
    private string _migrationsAssembly = typeof(Program).Assembly.GetName().Name ?? throw new ArgumentNullException("Migrations assembly cannot be null.");

    public string ConnectionString
    {
        get => _connectionString;
        set
        {
            // Здесь можно добавить валидацию, если нужно
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(value));
            _connectionString = value;
        }
    }

    public string MigrationsAssembly 
    { 
        get => _migrationsAssembly; 
        private set => _migrationsAssembly = value; 
    }
}
