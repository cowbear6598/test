namespace Acme.Common.Models;

// NOTE: pricing-service (Go, test2 repo) depends on the "tier" string values below.
// Do NOT rename them without coordinating with the pricing team.
public static class CustomerTier
{
    public const string Bronze = "Bronze";
    public const string Silver = "Silver";
    public const string Gold = "Gold";
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Tier { get; set; } = CustomerTier.Bronze;
}
