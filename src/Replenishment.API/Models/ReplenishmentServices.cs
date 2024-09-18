using PantsuTapPlayground.Replenishment.Api.Services;

namespace PantsuTapPlayground.Replenishment.Api.Models;

public class ReplenishmentServices(
    WalletService wallet,
    ILogger<ReplenishmentServices> logger)
{
    public WalletService Wallet { get; } = wallet;
    public ILogger<ReplenishmentServices> Logger { get; } = logger;
};