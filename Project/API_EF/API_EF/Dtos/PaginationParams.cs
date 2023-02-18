namespace API_EF.Dtos
{
    public class PaginationParams
    {
        private int _maxItemsPerPage = 10;
        private int itemsPerPage;

        public int Page { get; set; } = 1;

        public int ItemsPerPage
        {
            get => itemsPerPage;
            set => itemsPerPage = value > _maxItemsPerPage ? _maxItemsPerPage : value;
        }

    }
}
