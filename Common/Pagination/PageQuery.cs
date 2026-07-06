namespace Common.Pagination;

/// <summary>
/// 分頁查詢參數。
/// </summary>
public sealed class PageQuery
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; }
    public int PageSize { get; }

    public int Offset => (Page - 1) * PageSize;

    public PageQuery(int page = DefaultPage, int pageSize = DefaultPageSize)
    {
        Page = page < 1 ? DefaultPage : page;
        PageSize = pageSize < 1 ? DefaultPageSize
                 : pageSize > MaxPageSize ? MaxPageSize
                 : pageSize;
    }
}
