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
                var transfer = _cache.ExtractTransfer(request.Guid);
                if (transfer == null)
                {
                    _logger.LogError($"Transfer with signature {request.Guid} not found in cache.");
                    return;
                }

                var error = resp.Value.Error;
                if (error != null)
                {
                    transfer.Status = TransferStatus.Rejected;
                    _logger.LogError($"Transaction {request.Signature} failed: {error.Type}. Instruction error: {error.InstructionError?.ToString() ?? "None"}");
                }
                else
                {
                    transfer.Status = TransferStatus.Approved;
                    _logger.LogInformation($"Transaction {request.Signature} approved.");
                }

                _cache.UpdateTransfer(request.Guid, transfer);
            } catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the subscription for signature {Signature}.", request.Signature);
            }
        }, Commitment.Finalized);
    }
}
