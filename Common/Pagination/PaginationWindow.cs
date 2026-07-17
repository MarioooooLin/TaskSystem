namespace Common.Pagination;

public static class PaginationWindow
{
    public static IReadOnlyList<int> GetVisiblePages(int currentPage, int totalPages, int radius = 1)
    {
        if (totalPages <= 0)
            return [];

        return Enumerable.Range(1, totalPages)
            .Where(page => page == 1 || page == totalPages || Math.Abs(page - currentPage) <= radius)
            .ToArray();
    }
}
