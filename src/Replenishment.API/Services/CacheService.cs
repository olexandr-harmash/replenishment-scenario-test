using Microsoft.Extensions.Caching.Memory;
using PantsuTapPlayground.Replenishment.Api.Models;

namespace PantsuTapPlayground.Replenishment.Api.Services;

public class CacheService
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _options;
    private static readonly TimeSpan TransferExpiration = TimeSpan.FromDays(7);

    /// <summary>
    /// Конструктор для инициализации сервиса кэширования.
    /// </summary>
    /// <param name="cache">Объект кэша, предоставляемый через внедрение зависимостей (DI).</param>
    public CacheService(IMemoryCache cache)
    {
        _cache = cache;

        _options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TransferExpiration
        };
    }

    /// <summary>
    /// Сохраняет транзакцию в кэше по указанному публичному ключу пользователя.
    /// </summary>
    /// <param name="key">Ключ для сохранения транзакции.</param>
    /// <param name="tx">Транзакция в виде массива байт.</param>
    /// <returns>Возвращает сохранённый массив байт транзакции.</returns>
    public void PullTransfer(string key, Transfer t)
    {
        if (t == null)
        {
            throw new ArgumentNullException(nameof(Transfer), "Transfer cannot be null.");
        }

        if (_cache.TryGetValue(key, out _))
        {
            throw new InvalidOperationException("Transfer already exists in the cache.");
        }

        _cache.Set(key, t, _options);
    }

    public void UpdateTransfer(string key, Transfer t)
    {
        if (t == null)
        {
            throw new ArgumentNullException(nameof(Transfer), "Transfer cannot be null.");
        }

        if (!_cache.TryGetValue(key, out _))
        {
            throw new InvalidOperationException("Transfer not exists in the cache.");
        }

        _cache.Set(key, t, _options);
    }

    /// <summary>
    /// Получает транзакцию из кэша по ключу и удаляет из кеша.
    /// </summary>
    /// <param name="key">Ключ для поиска транзакции.</param>
    /// <returns>Массив байт, представляющий транзакцию.</returns>
    /// <exception cref="Exception">Выбрасывается, если транзакция не найдена в кэше.</exception>
    public Transfer ExtractTransfer(string key)
    {
        if (_cache.TryGetValue(key, out object? value))
        {
            if (value is Transfer tx)
            {
                return tx;
            }
            else
            {
                throw new InvalidCastException($"Value associated with key {key} is not a byte array.");
            }
        }

        throw new KeyNotFoundException($"Transfer with key {key} not found in the cache.");
    }

    public void RemoveTransfer(string key)
    {
        if (!KeyExists(key))
        {
            throw new KeyNotFoundException($"Transfer with key {key} not found in the cache.");
        }

        _cache.Remove(key);
    }

    public bool KeyExists(string key)
    {
        return _cache.TryGetValue(key, out _);
    }
}