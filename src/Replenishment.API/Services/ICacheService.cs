namespace PantsuTapPlayground.Replenishment.Api.Services;

public interface ICacheService
{
    /// <summary>
    /// Сохраняет транзакцию в кэше по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ для сохранения транзакции.</param>
    /// <param name="t">Транзакция.</param>
    void PullTransfer(Guid key, Transfer t);

    /// <summary>
    /// Обновляет транзакцию в кэше по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ для сохранения транзакции.</param>
    /// <param name="t">Транзакция.</param>
    void UpdateTransfer(Guid key, Transfer t);

    /// <summary>
    /// Получает транзакцию из кэша по ключу и удаляет её из кэша.
    /// </summary>
    /// <param name="key">Ключ для поиска транзакции.</param>
    /// <returns>Транзакция.</returns>
    Transfer ExtractTransfer(Guid key);

    /// <summary>
    /// Удаляет транзакцию из кэша.
    /// </summary>
    /// <param name="key">Ключ для поиска транзакции.</param>
    void RemoveTransfer(Guid key);

    /// <summary>
    /// Проверяет, существует ли ключ в кэше.
    /// </summary>
    /// <param name="key">Ключ для проверки.</param>
    /// <returns>Истина, если ключ существует.</returns>
    bool KeyExists(Guid key);
}
