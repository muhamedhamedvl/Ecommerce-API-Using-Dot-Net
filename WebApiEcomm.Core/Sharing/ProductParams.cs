namespace WebApiEcomm.Core.Sharing
{
    public class ProductParams : PaginationParams
    {
        public string sort { get; set; }
        public int? CategoryId { get; set; }
        public string Search { get; set; }

        // Keep PageNumber for backward compatibility
        public int PageNumber
        {
            get => Page;
            set => Page = value;
        }
    }
}
