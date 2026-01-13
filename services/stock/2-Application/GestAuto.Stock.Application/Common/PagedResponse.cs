namespace GestAuto.Stock.Application.Common;

public sealed record PaginationMetadata(int Page, int Size, int Total, int TotalPages);

public sealed record PagedResponse<T>(IReadOnlyList<T> Data, PaginationMetadata Pagination);
