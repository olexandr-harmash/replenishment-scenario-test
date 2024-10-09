namespace PantsuTapPlayground.Replenishment.Api.Models;

/// <summary>
/// Модель для представления информации о переводе средств в системе.
/// </summary>
public class Transfer
{
    /// <summary>
    /// Минимальная сумма перевода в лампортах (1 лампорт = 1/1,000,000 SOL).
    /// </summary>
    private readonly ulong _minAmount = 5_000_000;
    
    /// <summary>
    /// Сумма перевода в лампортах.
    /// </summary>
    private ulong _amount;

    /// <summary>
    /// Подпись транзакции.
    /// </summary>
    private string _signature = string.Empty;

    /// <summary>
    /// Адрес получателя.
    /// </summary>
    private string _to = string.Empty;

    /// <summary>
    /// Публичный ключ сервера, загружаемый из переменной окружения SERVER_PUBLIC_KEY.
    /// </summary>
    private readonly string _pubKey = Environment.GetEnvironmentVariable("SERVER_PUBLIC_KEY") ?? throw new Exception("SERVER_PUBLIC_KEY cannot be found from env.");

    /// <summary>
    /// Уникальный идентификатор транзакции.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Адрес отправителя.
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Текущий статус перевода.
    /// </summary>
    public TransferStatus Status { get; set; } = TransferStatus.Sending;

    /// <summary>
    /// Подпись транзакции.
    /// Значение не может быть null или пустым.
    /// </summary>
    /// <exception cref="ArgumentException">Бросается, если подпись null или пуста.</exception>
    public string? Signature
    {
        get => _signature;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Signature cannot be null or empty.");
            }
            _signature = value;
        }
    }

    /// <summary>
    /// Адрес получателя перевода.
    /// Адрес получателя должен совпадать с публичным ключом сервера.
    /// </summary>
    /// <exception cref="ArgumentException">Бросается, если адрес получателя не совпадает с публичным ключом сервера.</exception>
    public string To
    {
        get => _to;
        set
        {
            if (value == _pubKey)
            {
                _to = value;
            }
            else
            {
                throw new ArgumentException("The recipient address must be your public key.");
            }
        }
    }

    /// <summary>
    /// Сумма перевода в лампортах.
    /// Сумма должна быть не менее _minAmount лампортов.
    /// </summary>
    /// <exception cref="ArgumentException">Бросается, если сумма меньше минимального значения.</exception>
    public ulong Amount
    {
        get => _amount;
        set
        {
            if (value < _minAmount)
            {
                throw new ArgumentException($"Amount must be at least {_minAmount} lamports.");
            }
            _amount = value;
        }
    }
}
