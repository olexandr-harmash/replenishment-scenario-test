using Solnet.Rpc.Types;

namespace PantsuTapPlayground.Replenishment.Api.Commands;

public record SubscribeTransferCommandHandler : INotificationHandler<SubscribeTransferCommand>
{
    private readonly ICacheService _cache;
    private readonly IStreamingRpcClient _rpcClient;

    public SubscribeTransferCommandHandler(ICacheService cache, IStreamingRpcClient client)
    {
        _cache = cache;
        _rpcClient = client;
    }

    public async Task Handle(SubscribeTransferCommand request, CancellationToken cancellationToken)
    {
        var signature = request.Transfer.Signature;
        var _ = await _rpcClient.SubscribeSignatureAsync(signature, (state, resp) =>
        {
            var id = request.Transfer.Id;
            try
            {
                var transfer = _cache.ExtractTransfer(id);
                if (transfer == null)
                {
                    Log.Error("Transfer with signature {Guid} not found in cache.", request.Transfer.Id);
                    return;
                }

                var error = resp.Value.Error;
                if (error != null)
                {
                    transfer.Status = TransferStatus.Rejected;
                    Log.Error("Transaction {Signature} failed: {ErrorType}. Instruction error: {InstructionError}",
                        signature, error.Type, error.InstructionError?.ToString() ?? "None");
                }
                else
                {
                    transfer.Status = TransferStatus.Approved;
                    Log.Information("Transaction {Signature} approved.", signature);
                }

                _cache.UpdateTransfer(id, transfer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the subscription for signature {Signature}.", signature);
            }
        }, Commitment.Finalized);
    }
}
