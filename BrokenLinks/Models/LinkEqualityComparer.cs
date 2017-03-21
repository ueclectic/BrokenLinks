using System.Collections.Generic;

namespace BrokenLinks.Models
{
    class LinkEqualityComparer : IEqualityComparer<Link>
    {
        public bool Equals(Link link1, Link link2)
        {
            return link1!=null && link2!=null && link1.Url == link2.Url;
        }

        public int GetHashCode(Link link)
        {
            return link.Url.GetHashCode();
        }
    }
}
