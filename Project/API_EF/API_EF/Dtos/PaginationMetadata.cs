namespace API_EF.Dtos
{
    public class PaginationMetadata
    {
        public int CurrentPage { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PaginationMetadata(int totalCount, int currentPage, int itemsPerPage)
        {
            TotalCount = totalCount;
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage);
        }
    }
}
