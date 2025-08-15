namespace Features.Products.Domain;

public readonly struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }
    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim();
        Amount = amount;
    }
    public override string ToString() => $"{Amount:0.##} {Currency}";
}
