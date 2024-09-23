using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PantsuTapPlayground.Replenishment.Api.Commands;
using PantsuTapPlayground.Replenishment.Api.Dtos;
using PantsuTapPlayground.Replenishment.Api.Models;
using Solnet.Programs;
using Solnet.Rpc.Models;
using Solnet.Wallet;

namespace PantsuTapPlayground.Replenishment.Api.Apis;

/// <summary>
/// Defines the API endpoints for the Replenishment Service in Pantsu Tap.
/// This API allows users to create and execute replenishment transactions within the system.
/// </summary>
public static class ReplenishmentApi
{
    /// <summary>
    /// Maps version 1.0 of the Replenishment API.
    /// The API provides endpoints for creating and executing signed transactions.
    /// </summary>
    /// <param name="app">The endpoint route builder used to define routes for the API.</param>
    /// <returns>The endpoint route builder with the configured routes.</returns>
    public static IEndpointRouteBuilder MapReplenishmentApiV1(this IEndpointRouteBuilder app)
    {
        // Group the API endpoints under 'api/replenishment' with versioning support (1.0).
        var api = app.MapGroup("api/replenishment").HasApiVersion(1.0);

        // Endpoint to execute a signed replenishment transaction.
        api.MapPut("/transaction", ExecuteTransaction);

        return app;
    }

    /// <summary>
    /// Executes the signed replenishment transaction.
    /// This is a PUT request where the client sends the signed transaction for execution.
    /// The server verifies the signature and processes the transaction.
    /// </summary>
    /// <param name="authorizationHeader">Authorization header containing the bearer token.</param>
    /// <param name="request">The signed transaction sent by the client in Base64 format.</param>
    /// <param name="services">Service dependencies, such as logging and RPC client.</param>
    /// <returns>Returns the result of the transaction execution or a BadRequest in case of errors.</returns>
    public static async Task<Results<Ok, BadRequest<string>>> ExecuteTransaction(
        [FromHeader(Name = "Authorization")] string authorizationHeader,
        [FromBody] ExecuteTransferTransactionDto request,
        [AsParameters] ReplenishmentServices services)
    {
        // Extract Bearer token from authorization header
        var token = authorizationHeader?.Replace("Bearer ", string.Empty);

        if (string.IsNullOrEmpty(token))
        {
            return TypedResults.BadRequest("Authorization header is missing or invalid.");
        }

        if (services.Cache.KeyExists(token))
        {
            return TypedResults.BadRequest("Подождите выполнения предыдущей транзакции (тестовое окружение).");
        }

        // Deserialize the transaction and compile the message
        var msgBytes = Transaction
            .Deserialize(request.Base64TransactionData)
            .CompileMessage();

        var msg = Message.Deserialize(msgBytes);

        // Decode transaction instructions
        var instructions = InstructionDecoder.DecodeInstructions(msg);

        // Search for the Transfer instruction
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

            // Log transfer details before sending
            services.Logger.LogInformation(
                "Transfer sending:\n From: {From}\n To: {To}\n Amount: {Amount}",
                transferInstruction.From,
                transferInstruction.To,
                transferInstruction.Amount);

            // Save the transfer to cache
            services.Cache.PullTransfer(token, transferInstruction);
        }
        catch (InvalidOperationException)
        {
            // Log an error if the instruction was not found or multiple were found
            services.Logger.LogError("More than one or no Transfer instructions were found.");
            return TypedResults.BadRequest("Transfer instruction not found or ambiguous.");
        }

        // Send the transaction and retrieve the signature
        var result = await services.RpcCilent.SendTransactionAsync(request.Base64TransactionData);

        if (!result.WasSuccessful)
        {
            // Log an error if the transaction failed to send
            services.Logger.LogError("Error sending the transaction.");
            return TypedResults.BadRequest("Error sending the transaction.");
        }

        var requestTransfer = new SubscribeTransferCommand(result.Result, token);

        services.Logger.LogInformation(
            "Sending command: {CommandName}: ({@Command})",
            nameof(requestTransfer),
            requestTransfer);

        await services.Mediator.Publish(requestTransfer);

        return TypedResults.Ok();
    }
}
