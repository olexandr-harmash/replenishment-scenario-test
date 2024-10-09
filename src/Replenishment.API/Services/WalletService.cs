using System.Text;
using Solnet.Wallet;
using Solnet.KeyStore;
using Solnet.Wallet.Bip39;

namespace PantsuTapPlayground.Replenishment.Api.Services;
/// <summary>
/// Сервис для управления кошельками, включая инициализацию кошельков из хранилища ключей.
/// </summary>
public class WalletService : IWalletService
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

        _keypairFile = Environment.GetEnvironmentVariable("SERVER_KEY_PAIR_FILE") ?? throw new InvalidOperationException("Environment variable 'SERVER_KEY_PAIR_FILE' is not set.");

        _passPhrase = Environment.GetEnvironmentVariable("SERVER_PASS_PHRASE") ?? throw new InvalidOperationException("Environment variable 'SERVER_PASS_PHRASE' is not set.");

        _keypairPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _keypairFile);

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
            if (!File.Exists(_keypairPath))
            {
                _logger.LogError($"Keypair file not found at path: {_keypairPath}");
                throw new FileNotFoundException("Keypair file not found");
            }

            var encryptedKeystoreJson = File.ReadAllText(_keypairPath);
            var decryptedKeystore = _keystoreService.DecryptKeyStoreFromJson(_passPhrase, encryptedKeystoreJson);

            var mnemonicString = Encoding.UTF8.GetString(decryptedKeystore);
            var mnemonic = new Mnemonic(mnemonicString);
            return new Wallet(mnemonic);
        }
        catch (FileNotFoundException fnfe)
        {
            _logger.LogError(fnfe.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }
}
