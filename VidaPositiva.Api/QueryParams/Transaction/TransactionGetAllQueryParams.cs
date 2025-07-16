namespace VidaPositiva.Api.QueryParams.Transaction;

public record TransactionGetAllQueryParams
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
};