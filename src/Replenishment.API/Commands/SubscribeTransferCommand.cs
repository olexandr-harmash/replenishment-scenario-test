using MediatR;

namespace PantsuTapPlayground.Replenishment.Api.Commands;

public record SubscribeTransferCommand(string Signature, string Id) : INotification;