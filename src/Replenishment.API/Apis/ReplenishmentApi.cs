using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PantsuTapPlayground.Replenishment.Api.Dtos;
using PantsuTapPlayground.Replenishment.Api.Models;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;

// Defines the API endpoints for the Replenishment Service in Pantsu Tap.
// This API allows users to create and execute replenishment transactions within the system.
// Developed by Olexandr Harmash on [date]. For detailed documentation and task information, refer to the Confluence page and Jira task links below.
// TODO: 
// - Implement JWT authorization for endpoints.
// - Store transaction data and implement further validation for transactions.
// 
// For more details, see:
// - Solnet GitHub library: [Solnet GitHub](https://github.com/solana-labs/solnet)

namespace PantsuTapPlayground.Replenishment.Api.Apis;

/// <summary>
/// Defines the API endpoints for the Replenishment Service in Pantsu Tap.
/// This API allows users to create and execute replenishment transactions within the system.
/// </summary>
public static class ReplenishmentApi
{
    /// <summary>
    /// Maps the version 1.0 of the Replenishment API. 
    /// The API provides endpoints for creating and executing signed transactions.
    /// </summary>
    /// <param name="app">The endpoint route builder used to define routes for the API.</param>
    /// <returns>The endpoint route builder with the configured routes.</returns>
    public static IEndpointRouteBuilder MapReplenishmentApiV1(this IEndpointRouteBuilder app)
    {
        // Group the API endpoints under 'api/replenishment' with versioning support (1.0).
        var api = app.MapGroup("api/replenishment").HasApiVersion(1.0);

        // Endpoint to create a new replenishment transaction.
        api.MapPost("/transaction", CreateTransaction);

        // Endpoint to execute a signed replenishment transaction.
        api.MapPut("/transaction", ExecuteSignedTransaction);

        return app;
    }

    /// <summary>
    /// Creates a new replenishment transaction for the user.
    /// This is a POST request where the client sends transaction details such as public keys and amount.
    /// The server returns the transaction data to be signed by the client.
    /// </summary>
    /// <param name="request">The transaction details sent by the client.</param>
    /// <param name="services">The replenishment service dependencies.</param>
    /// <returns>Returns the created transaction details or a BadRequest in case of errors.</returns>
    public static async Task<Results<Ok<TransactionDetailsDto>, BadRequest<string>>> CreateTransaction(
        [FromHeader(Name = "Authorization")] string authorizationHeader,
        [FromBody] TransactionRequestDto request,
        [AsParameters] ReplenishmentServices services)
    {
        //todo:jwt authorization
        var jwtToken = authorizationHeader?.Replace("Bearer ", string.Empty);

        if (string.IsNullOrEmpty(jwtToken))
        {
            return TypedResults.BadRequest("Bearer must be in \"Authorization\" header.");
        }

        var wallet = services.Wallet.InitializeWallet();

        IRpcClient rpcClient = ClientFactory.GetClient(Cluster.DevNet);

        RequestResult<ResponseValue<LatestBlockHash>> blockHash = rpcClient.GetLatestBlockHash();

        services.Logger.LogInformation($"BlockHash >> {blockHash.Result.Value.Blockhash}");
        
        var account = wallet.Account;

        byte[] tx = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(account)
                .AddInstruction(SystemProgram.Transfer(account.PublicKey, account.PublicKey, request.Lamports))
                .Build(account);

        services.Logger.LogInformation($"Tx base64: {Convert.ToBase64String(tx)}");

        RequestResult<ResponseValue<SimulationLogs>> txSim = rpcClient.SimulateTransaction(tx);

        services.Logger.LogInformation($"Transaction Simulation:\n\tError: {txSim.Result.Value.Error}\n\tLogs: {string.Join("\n", txSim.Result.Value.Logs)}\n");
        
        // TODO: Implement actual logic for creating the transaction.
        return TypedResults.Ok(new TransactionDetailsDto(tx));
    }

    /// <summary>
    /// Executes the signed replenishment transaction.
    /// This is a PUT request where the client sends the signed transaction for execution.
    /// The server verifies the signature and processes the transaction.
    /// </summary>
    /// <param name="request">The signed transaction sent by the client.</param>
    /// <param name="services">The replenishment service dependencies.</param>
    /// <returns>Returns the result of the transaction execution or a BadRequest in case of errors.</returns>
    public static async Task<Results<Ok<TransactionResultDto>, BadRequest<string>>> ExecuteSignedTransaction(
        [FromHeader(Name = "Authorization")] string authorizationHeader,
        [FromBody] TransactionSignedDto request,
        [AsParameters] ReplenishmentServices services)
    {
        var jwtToken = authorizationHeader?.Replace("Bearer ", string.Empty);

        if (string.IsNullOrEmpty(jwtToken))
        {
            return TypedResults.BadRequest("Bearer must be in \"Authorization\" header.");
        }

        IRpcClient rpcClient = ClientFactory.GetClient(Cluster.DevNet);

        RequestResult<string> firstSig = rpcClient.SendTransaction(request.Transaction);

        services.Logger.LogInformation($"First Tx Signature: {firstSig.Result}");
        
        return TypedResults.Ok(new TransactionResultDto(firstSig.Result));
    }
}
