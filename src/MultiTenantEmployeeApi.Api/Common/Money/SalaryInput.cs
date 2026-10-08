namespace MultiTenantEmployeeApi.Api.Common.Money;

public sealed class SalaryInput
{
    public int AmountMinor { get; init; }

    public string CurrencyCode { get; init; } =
        string.Empty;
}