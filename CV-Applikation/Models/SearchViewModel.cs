namespace CV_Applikation.Models
{
    public class SearchViewModel
    {
        public string SearchString { get; set; }    
        public List<SearchResult> Results { get; set; } = new List<SearchResult>();

    }
}
