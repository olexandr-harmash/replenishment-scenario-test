using System.Text;
using Solnet.Wallet;
using Solnet.KeyStore;
using Solnet.Wallet.Bip39;

namespace PantsuTapPlayground.Replenishment.Api.Services;
/// <summary>
/// Сервис для управления кошельками, включая инициализацию кошельков из хранилища ключей.
/// </summary>
public class WalletService
{
    private readonly string _keypairFile;
    private readonly string _keypairPath;
    private readonly string _passPhrase;
    private readonly ILogger<WalletService> _logger;
    private readonly SecretKeyStoreService _keystoreService;

    /// <summary>
    /// Конструктор для инициализации сервиса WalletService.
    /// </summary>
    /// <param name="options">Параметры для настройки сервиса.</param>
    /// <param name="logger">Сервис для логирования.</param>
    /// <param name="keystoreService">Сервис для работы с хранилищем ключей.</param>
    public WalletService(ILogger<WalletService> logger)
    {
        _logger = logger;

        // Получение пути к файлу ключа из переменной окружения
        _keypairFile = Environment.GetEnvironmentVariable("SERVER_KEY_PAIR_FILE") 
            ?? throw new InvalidOperationException("Environment variable 'SERVER_KEY_PAIR_FILE' is not set.");

        // Получение пароля из переменной окружения
        _passPhrase = Environment.GetEnvironmentVariable("SERVER_PASS_PHRASE") 
            ?? throw new InvalidOperationException("Environment variable 'SERVER_PASS_PHRASE' is not set.");

        // Формирование полного пути к файлу ключа
        _keypairPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _keypairFile);

        // Инициализация сервиса для работы с хранилищем ключей
        _keystoreService = new SecretKeyStoreService();

        _logger.LogDebug("Passphrase successfully retrieved.");
    }

    /// <summary>
    /// Инициализирует кошелек из хранилища ключей.
    /// </summary>
    /// <returns>Инициализированный объект <see cref="Wallet"/>.</returns>
    /// <exception cref="FileNotFoundException">Бросается, если файл хранилища ключей не найден.</exception>
    /// <exception cref="Exception">Бросается при возникновении ошибок во время расшифровки или инициализации кошелька.</exception>
    public Wallet InitializeWallet()
    {
        try
        {
            // Проверяем наличие файла хранилища ключей
            if (!File.Exists(_keypairPath))
            {
                _logger.LogError($"Keypair file not found at path: {_keypairPath}");
                throw new FileNotFoundException("Keypair file not found", _keypairPath);
            }

            // Чтение зашифрованного JSON-файла с ключами
            var encryptedKeystoreJson = File.ReadAllText(_keypairPath);

            // Расшифровка ключа с использованием passphrase
            var decryptedKeystore = _keystoreService.DecryptKeyStoreFromJson(_passPhrase, encryptedKeystoreJson);

            // Конвертация расшифрованного хранилища в строку
            var mnemonicString = Encoding.UTF8.GetString(decryptedKeystore);

            // Инициализация кошелька с использованием мнемонической фразы
            var mnemonic = new Mnemonic(mnemonicString);
            return new Wallet(mnemonic);
        }
        catch (FileNotFoundException fnfe)
        {
            // Логируем и пробрасываем исключение, если файл не найден
            _logger.LogError(fnfe, "Keypair file was not found.");
            throw;
        }
        catch (Exception ex)
        {
            // Логируем и пробрасываем любые другие ошибки
            _logger.LogError(ex, "An error occurred during wallet initialization.");
            throw;
        }
    }
}
