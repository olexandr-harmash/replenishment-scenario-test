using Microsoft.Extensions.Caching.Memory;
using PantsuTapPlayground.Replenishment.Api.Models;

namespace PantsuTapPlayground.Replenishment.Api.Services;

public class CacheService
{
    private readonly IMemoryCache _cache; // Интерфейс для работы с кэшем
    private readonly MemoryCacheEntryOptions _options; // Опции для конфигурации кэширования
    private static readonly TimeSpan TransferExpiration = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Конструктор для инициализации сервиса кэширования.
    /// </summary>
    /// <param name="cache">Объект кэша, предоставляемый через внедрение зависимостей (DI).</param>
    public CacheService(IMemoryCache cache)
    {
        _cache = cache;

        // Устанавливаем опции для кэширования, где срок хранения в кэше - 10 секунд.
        _options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TransferExpiration // 10 секунд
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

        // Проверяем, существует ли уже транзакция в кэше
        if (_cache.TryGetValue(key, out _))
        {
            //throw new InvalidOperationException("Transfer already exists in the cache.");
        }

        // Сохраняем транзакцию в кэше с использованием опций кэширования
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
        // Пытаемся получить значение из кэша
        if (_cache.TryGetValue(key, out object? value))
        {
            if (value is Transfer tx)
            {
                return tx; // Возвращаем найденную транзакцию
            }
            else
            {
                throw new InvalidCastException($"Value associated with key {key} is not a byte array.");
            }
        }

        // Если значение не найдено, выбрасываем исключение
        throw new KeyNotFoundException($"Transfer with key {key} not found in the cache.");
    }

    public void RemoveTransfer(string key)
    {
        // Проверяем, существует ли транзакция с указанным ключом
        if (!KeyExists(key))
        {
            throw new KeyNotFoundException($"Transfer with key {key} not found in the cache.");
        }

        // Удаляем транзакцию из кеша
        _cache.Remove(key);
    }

    public bool KeyExists(string key)
    {
        return _cache.TryGetValue(key, out _);
    }
}