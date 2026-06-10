using System.ComponentModel.DataAnnotations;

public class PaymentOptions
{
    // The Gateway URL is strictly required
    [Required(ErrorMessage = "The GatewayUrl field is required.")]
    public required string GatewayUrl { get; init; }

    // Enforce that deposits must be within a realistic threshold (100 to 100,000 Ethiopian Birr)
    [Range(100, 100000, ErrorMessage = "MaxDepositBirr must be between 100 and 100,000 Birr.")]
    public decimal MaxDepositBirr { get; init; }
}