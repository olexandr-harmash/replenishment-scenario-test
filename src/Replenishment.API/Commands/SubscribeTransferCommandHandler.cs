using MediatR;
using PantsuTapPlayground.Replenishment.Api.Models;
using PantsuTapPlayground.Replenishment.Api.Services;
using Solnet.Rpc;
using Solnet.Rpc.Types;

namespace PantsuTapPlayground.Replenishment.Api.Commands;

public record SubscribeTransferCommandHandler : INotificationHandler<SubscribeTransferCommand>
{
    private readonly CacheService _cache;
    private readonly IStreamingRpcClient _rpcClient;
    private readonly ILogger<SubscribeTransferCommandHandler> _logger;

    public SubscribeTransferCommandHandler(CacheService cache, IStreamingRpcClient client, ILogger<SubscribeTransferCommandHandler> logger)
    {
        _cache = cache;
        _logger = logger;
        _rpcClient = client;
    }

    public async Task Handle(SubscribeTransferCommand request, CancellationToken cancellationToken)
    {
        // Подписываемся на событие подтверждения по сигнатуре транзакции
        var _ = await _rpcClient.SubscribeSignatureAsync(request.Signature, (state, resp) =>
        {
            try {
                // Получаем информацию о транзакции из кэша
                var transfer = _cache.ExtractTransfer(request.Id);
                if (transfer == null)
                {
                    _logger.LogError($"Transfer with signature {request.Id} not found in cache.");
                    return;
                }

                // Проверяем наличие ошибок в ответе
                var error = resp.Value.Error;
                if (error != null)
                {
                    // Если есть ошибка, логируем её и помечаем статус транзакции как Rejected
                    transfer.Status = TransferStatus.Rejected;
                    _logger.LogError($"Transaction {request.Signature} failed: {error.Type}. Instruction error: {error.InstructionError?.ToString() ?? "None"}");
                }
                else
                {
                    // Если ошибок нет, помечаем транзакцию как Approved
                    transfer.Status = TransferStatus.Approved;
                    _logger.LogInformation($"Transaction {request.Signature} approved.");
                }

                // Обновляем информацию о транзакции в кэше
                _cache.PullTransfer(request.Id, transfer);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the subscription for signature {Signature}.", request.Signature);
            }
        }, Commitment.Finalized); // Используем Commitment.Finalized для полной финализации транзакции
    }
}
