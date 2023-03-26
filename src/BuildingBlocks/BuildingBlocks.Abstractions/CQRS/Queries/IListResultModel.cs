namespace BuildingBlocks.Abstractions.CQRS.Queries;

public interface IListResultModel<T>
{
    IList<T> Items { get; init; }
    long TotalItems { get; init; }
    int Page { get; init; }
    int PageSize { get; init; }
}
