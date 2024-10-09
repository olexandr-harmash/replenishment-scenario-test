namespace PantsuTapPlayground.Replenishment.Api.Services;

public class TransferService : ITransferService
{
    public TransferService() {}

    public Transfer GetTransferFromInstructions(List<DecodedInstruction> instructions)
    {
        var data = instructions.Single(i => i.InstructionName == nameof(Transfer));

        var transferInstruction = new Transfer
        {
            From = (PublicKey)data.Values["From Account"],
            To = (PublicKey)data.Values["To Account"],
            Amount = (ulong)data.Values["Amount"]
        };

        Log.Information(
            "Transfer sending:\n From: {From}\n To: {To}\n Amount: {Amount}",
            transferInstruction.From,
            transferInstruction.To,
            transferInstruction.Amount);

        return transferInstruction;
    }
}