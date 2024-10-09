using MediatR;

namespace PantsuTapPlayground.Replenishment.Api.Apis;

public class ReplenishmentServices(
    ICacheService cache,
    IMediator mediator,
    IWalletService wallet,
    ILogger<ReplenishmentServices> logger,
    ITransferService transfer,
    IRpcClient rpcCilent)
{
    public IMediator Mediator { get; set; } = mediator;
    public ICacheService Cache { get; } = cache;
    public IWalletService Wallet { get; } = wallet;
    public IRpcClient RpcCilent { get; } = rpcCilent;
    public ILogger<ReplenishmentServices> Logger { get; } = logger;
    public ITransferService Transfer { get; } = transfer;
};