namespace MultiTenantEmployeeApi.Api.ValueObjects;

public sealed class Money
{
    private Money()
    {
    }

    public Money(
        int amountMinor,
        string currencyCode)
    {
        if (amountMinor < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amountMinor),
                "Amount minor cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException(
                "Currency code is required.",
                nameof(currencyCode));
        }

        AmountMinor = amountMinor;
        CurrencyCode = currencyCode
            .Trim()
            .ToUpperInvariant();
    }

    public int AmountMinor { get; private set; }

    public string CurrencyCode { get; private set; } =
        string.Empty;
}