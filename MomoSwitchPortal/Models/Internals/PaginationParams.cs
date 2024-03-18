namespace MomoSwitchPortal.Models.Internals
{
    public class PaginationParams
    {
        private const int _MaxItemsPerPage = 50;
        private int _ItemsPerPage;
        public int Page { get; set; } = 1;
        public int ItemsPerPage
        {
            get => _ItemsPerPage;
            set => _ItemsPerPage = value > _MaxItemsPerPage ? _MaxItemsPerPage : value;
        }
    }



    public class PaginationMetaData
    {
        private int StartPage1;
        private int EndPage1;
        public PaginationMetaData(int totalCount, int currentPage, int itemPerPage)
        {
            CurrentPage = currentPage;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)itemPerPage);



            StartPage1 = currentPage - 5;
            EndPage1 = currentPage + 4;

            if (StartPage1 <= 0)
            {
                EndPage1 = EndPage1 - (StartPage1 - 1);
                StartPage1 = 1;
            }

            if (EndPage1 > TotalPages)
            {
                EndPage1 = TotalPages;
                if (EndPage1 > 10)
                    StartPage1 = EndPage1 - 9;

            }

        }
        public int CurrentPage { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public int StartPage => StartPage1;
        public int EndPage => EndPage1;
    }
}
