namespace BrokenLinks.Models
{
    public class Link
    {
        public string Location { get; private set; }
        public string Url { get; private set; }

        public Link(string location, string url)
        {
            Url = url;
            Location = location;
        }
    }
}
