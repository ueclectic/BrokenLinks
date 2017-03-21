using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BrokenLinks.Models
{
    public class Parser
    {
        private string _url;
        private string _htmlBody;
        public Parser(string url)
        {
            _url = url;
        }

        public async Task<string> GetPageStatus()
        {
            string responseStatus = "Error";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(WebUtility.HtmlDecode(_url));
                    responseStatus = response.StatusCode.ToString();
                    response.EnsureSuccessStatusCode();
                    _url = response.RequestMessage.RequestUri.AbsoluteUri;
                    if (response.Content.Headers.ContentType.MediaType.ToString().Contains("text"))
                    {
                        var body = await response.Content.ReadAsByteArrayAsync();
                        _htmlBody = Encoding.UTF8.GetString(body, 0, body.Length - 1);
                    }
                }
                catch (Exception e)
                {
                    responseStatus = e.Message;
                }
            }

            //PrintLog(url, responseStatus);
            return responseStatus;
        }

        private Uri GetBase(string htmlBody, string url)
        {
            Uri baseUri = SearchBase(@"<base\s+.*?href=\s*""([^""]*)""", htmlBody, url);

            if (baseUri == null)
            {
                baseUri = SearchBase(@"<base\s+.*?href=\s*'([^']*)'", htmlBody, url);
            }

            if (baseUri == null && !Uri.TryCreate(url, UriKind.Absolute, out baseUri))
            {
                return null;
            }

            return baseUri;
        }

        private Uri SearchBase(string regexp, string htmlBody, string url)
        {
            try
            {
                Uri host;
                if (!Uri.TryCreate(url, UriKind.Absolute, out host))
                    return null;

                string baseLink = Regex.Match(htmlBody, regexp, RegexOptions.Singleline).Groups[1].Value;
                Uri baseUri;
                if (!string.IsNullOrWhiteSpace(baseLink))
                {
                    if (Uri.TryCreate(host, baseLink, out baseUri))
                    {
                        return baseUri;
                    }
                }
            }
            catch { }

            return null;
        }

        public async Task<HashSet<string>> FindLinks()
        {

            if (string.IsNullOrWhiteSpace(_htmlBody))
            {
                return null;
            }

            HashSet<string> urls = null;

            await Task.Run(() =>
            {
                try
                {
                    Uri baseUri = GetBase(_htmlBody, _url);
                    if (baseUri != null)
                    {
                        MatchCollection links = Regex.Matches(_htmlBody, @"<a\s+(?:[^>]*?\s+)?href=\s*""([^""]*)""", RegexOptions.Singleline);
                        urls = ProcessFindedUrls(baseUri, links);

                        MatchCollection links2 = Regex.Matches(_htmlBody, @"<a\s+.*?href=\s*'([^']*)'", RegexOptions.Singleline);
                        var foundedUrls = ProcessFindedUrls(baseUri, links2);
                        urls.UnionWith(foundedUrls);
                    }
                }
                catch { }
            });

            return urls;
        }

        private HashSet<string> ProcessFindedUrls(Uri baseUri, MatchCollection links)
        {
            HashSet<string> searchedLinks = new HashSet<string>();

            foreach (Match link in links)
            {
                string linkString = link.Groups[1].Value;
                Uri newUri;
                if (Uri.TryCreate(baseUri, linkString, out newUri))
                {
                    if (newUri.Host == baseUri.Host && !searchedLinks.Contains<string>(newUri.AbsoluteUri))
                    {
                        searchedLinks.Add(newUri.AbsoluteUri);
                    }
                }
                // what if not???
            }

            return searchedLinks;
        }
    }
}
