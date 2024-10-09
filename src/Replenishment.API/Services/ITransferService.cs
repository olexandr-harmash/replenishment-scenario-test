namespace PantsuTapPlayground.Replenishment.Api.Services;

public interface ITransferService
{
    Transfer GetTransferFromInstructions(List<DecodedInstruction> instructions);
}