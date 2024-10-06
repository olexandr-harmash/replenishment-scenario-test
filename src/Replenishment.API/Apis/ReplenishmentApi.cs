using Solnet.Rpc.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

using Log = Serilog.Log;
using System.Security.Claims;

namespace PantsuTapPlayground.Replenishment.Api.Apis;

public static class ReplenishmentApi
{
    public static IEndpointRouteBuilder MapReplenishmentApiV1(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/replenishment").HasApiVersion(1.0);
        api.MapPost("/transaction", ExecuteTransaction);
        return app;
    }

    public static async Task<Results<Ok, BadRequest<string>>> ExecuteTransaction(
        [FromBody] ExecuteTransferTransactionDto request,
        [AsParameters] ReplenishmentServices services)
    {
        var msgBytes = Transaction.Deserialize(request.Base64TransactionData).CompileMessage();
        var msg = Message.Deserialize(msgBytes);

        var instructions = InstructionDecoder.DecodeInstructions(msg);

        Transfer transferInstruction;
        try
        {
            transferInstruction = services.Transfer.GetTransferFromInstructions(instructions);
            services.Cache.PullTransfer(transferInstruction.Id, transferInstruction);
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while retrieving transfer instruction.");
            return TypedResults.BadRequest("Transfer instruction not found or ambiguous.");
        }

        var result = await services.RpcCilent.SendTransactionAsync(request.Base64TransactionData);
        if (!result.WasSuccessful)
        {
            Log.Error("Error sending the transaction: {Reason}", result.Reason);
            return TypedResults.BadRequest("Error sending the transaction.");
        }

        transferInstruction.Signature = result.Result;
        var requestTransfer = new SubscribeTransferCommand(transferInstruction);

        Log.Information(
            "Sending command: {CommandName}: ({@Command})",
            nameof(requestTransfer),
            requestTransfer);

        await services.Mediator.Publish(requestTransfer);

        return TypedResults.Ok();
    }
}
