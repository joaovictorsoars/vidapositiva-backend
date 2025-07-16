using System.Text.Json.Serialization;

namespace VidaPositiva.Api.Enums.Transaction;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionTypeEnum
{
    Income,
    Expense,
    Transfer
}