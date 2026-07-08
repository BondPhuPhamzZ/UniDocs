namespace UniDocs.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string UrlTemplate { get; set; }
    }
}
