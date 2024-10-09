namespace PantsuTapPlayground.Replenishment.Api.Models;

public class Transfer
{
    private ulong _minAmount = 5_000_000;
    private ulong _amount;
    private string _signature = string.Empty;
    private string _to = string.Empty;
    public Guid Id = new Guid();
    public string From { get; set; } = string.Empty;
    public TransferStatus Status { get; set; } = TransferStatus.Sending;

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
            if (value < _minAmount)
            {
                throw new ArgumentException("Amount must be at least 5 000 000 lamports.");
            }
            _amount = value;
        }
    }
}
