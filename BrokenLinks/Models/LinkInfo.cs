namespace BrokenLinks.Models
{
    public class LinkInfo:Link
    {
        public string Status { get; set; }

        public LinkInfo() : base("", "") { }
        public LinkInfo(Link link):base(link.Location, link.Url) {}
        public LinkInfo(string location, string url, string status):base(location, url)
        {
            Status = status;
        }


        public override string ToString()
        {
            return $"Link: {Url}\nLocation: {Location}\nStatus: {Status}\n\n";
        }
    }
}
