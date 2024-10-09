using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PantsuTapPlayground.Replenishment.Api.Commands;
using PantsuTapPlayground.Replenishment.Api.Dtos;
using PantsuTapPlayground.Replenishment.Api.Models;
using Solnet.Programs;
using Solnet.Rpc.Models;
using Solnet.Wallet;

namespace PantsuTapPlayground.Replenishment.Api.Apis;

public static class ReplenishmentApi
{
    public static IEndpointRouteBuilder MapReplenishmentApiV1(this IEndpointRouteBuilder app)
    {

        var api = app.MapGroup("api/replenishment").HasApiVersion(1.0);

        api.MapPut("/transaction", ExecuteTransaction);

        return app;
    }

    public static async Task<Results<Ok, BadRequest<string>>> ExecuteTransaction(
        [FromHeader(Name = "Authorization")] string authorizationHeader,
        [FromBody] ExecuteTransferTransactionDto request,
        [AsParameters] ReplenishmentServices services)
    {
        var token = authorizationHeader?.Replace("Bearer ", string.Empty);

        if (string.IsNullOrEmpty(token))
        {
            return TypedResults.BadRequest("Authorization header is missing or invalid.");
        }

        var guid = Guid.NewGuid();

        var msgBytes = Transaction.Deserialize(request.Base64TransactionData).CompileMessage();
        var msg = Message.Deserialize(msgBytes);

        var instructions = InstructionDecoder.DecodeInstructions(msg);

        Transfer transferInstruction;
        try
        {
            var data = instructions.Single(i => i.InstructionName == nameof(Transfer));

            transferInstruction = new Transfer
            {
                From = (PublicKey)data.Values["From Account"],
                To = (PublicKey)data.Values["To Account"],
                Amount = (ulong)data.Values["Amount"]
            };

            services.Logger.LogInformation(
                "Transfer sending:\n From: {From}\n To: {To}\n Amount: {Amount}",
                transferInstruction.From,
                transferInstruction.To,
                transferInstruction.Amount);

            services.Cache.PullTransfer(guid.ToString(), transferInstruction);
        }
        catch (InvalidOperationException e)
        {
            services.Logger.LogError(e.Message);
            return TypedResults.BadRequest("Transfer instruction not found or ambiguous.");
        }

        var result = await services.RpcCilent.SendTransactionAsync(request.Base64TransactionData);

        if (!result.WasSuccessful)
        {
            services.Logger.LogError(result.Reason);
            return TypedResults.BadRequest("Error sending the transaction.");
        }

        var requestTransfer = new SubscribeTransferCommand(result.Result, guid.ToString());

        services.Logger.LogInformation(
            "Sending command: {CommandName}: ({@Command})",
            nameof(requestTransfer),
            requestTransfer);

        await services.Mediator.Publish(requestTransfer);

        return TypedResults.Ok();
    }
}
