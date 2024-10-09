namespace PantsuTapPlayground.Replenishment.Api.Commands;

public record SubscribeTransferCommand(Transfer Transfer) : INotification;