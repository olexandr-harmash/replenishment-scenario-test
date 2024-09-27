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
            if (value == Environment.GetEnvironmentVariable("SERVER_PUBLIC_KEY"))
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
            if (value < 5_000_000)
            {
                throw new ArgumentException("Amount must be at least 5 000 000 lamports.");
            }
            _amount = value;
        }
    }

    public TransferStatus Status { get; set; } = TransferStatus.Sending;
}
