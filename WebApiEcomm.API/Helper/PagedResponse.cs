namespace WebApiEcomm.API.Helper
{
    public class PagedResponse<T> where T : class
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public IEnumerable<T> Data { get; set; }

        public PagedResponse(int page, int pageSize, int totalCount, IEnumerable<T> data)
        {
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            HasPrevious = page > 1;
            HasNext = page < TotalPages;
            Data = data;
        }
    }
}
