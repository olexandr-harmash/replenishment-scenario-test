using MediatR;
using PantsuTapPlayground.Replenishment.Api.Services;
using Solnet.Rpc;

namespace PantsuTapPlayground.Replenishment.Api.Apis;

public class ReplenishmentServices(
    CacheService cache,
    IMediator mediator,
    WalletService wallet,
    ILogger<ReplenishmentServices> logger)
{
    public IMediator Mediator { get; set; } = mediator;
    public CacheService Cache { get; } = cache;
    public WalletService Wallet { get; } = wallet;
    public IRpcClient RpcCilent { get; } = ClientFactory.GetClient(Cluster.DevNet);
    public ILogger<ReplenishmentServices> Logger { get; } = logger;
};