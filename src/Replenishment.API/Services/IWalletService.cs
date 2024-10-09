namespace PantsuTapPlayground.Replenishment.Api.Services;

/// <summary>
/// Интерфейс для сервиса управления кошельками.
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Инициализирует кошелек из хранилища ключей.
    /// </summary>
    /// <returns>Инициализированный объект <see cref="Wallet"/>.</returns>
    Wallet InitializeWallet();
}
