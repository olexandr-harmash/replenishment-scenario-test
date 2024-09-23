namespace PantsuTapPlayground.Replenishment.Api.Models;

public class Transfer
{
    private ulong _amount;
    private string? _signature;

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

    public string From { get; set; } = string.Empty;
    private string _to = string.Empty;

    public string To
    {
        get => _to;
        set
        {
            // Устанавливаем значение To только если это ваш публичный ключ
            if (value == "EsPxEpdzie7F9fFQPpCQyxV7zEJPAjrVSASo7kAhNyov")
            {
                _to = value;
            }
            else
            {
                throw new ArgumentException("The recipient address must be your public key.");
            }
        }
    }

    public ulong Amount
    {
        get => _amount;
        set
        {
            // Проверяем, что сумма не меньше 100 грн в лампортах
            if (value < 1_000_000) // 100 грн = 100_000_000 лампорты (если 1 лампорт = 0.000000001 SOL)
            {
                throw new ArgumentException("Amount must be at least 100 грн in lamports.");
            }
            _amount = value;
        }
    }

    public TransferStatus Status { get; set; } = TransferStatus.Sending;
}
