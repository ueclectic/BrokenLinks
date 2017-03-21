using BrokenLinks.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using System.Linq;
using Windows.UI.Popups;
using System.Text;
using System.Collections.ObjectModel;

namespace BrokenLinks.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public readonly string pageTitle = "Broken links";
        HashSet<string> _parsedPages;
        HashSet<Link> _pagesToParse;
        ObservableCollection<LinkInfo> _deadLinks;

        Object writeLock = new Object();

        public MainViewModel()
        {
            Speed = 3;

            Parse = new RelayCommand<string>(OnParse);
            Cancel = new RelayCommand<string>(OnCancel);

            CanParse = true;
            CanCancel = false;

            Result = "";
            // BadLinks = "";

            Init();
        }


        private bool _canParse;
        public bool CanParse
        {
            get
            {
                return _canParse;
            }
            set
            {
                _canParse = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsCancelling");
            }
        }

        private void Init()
        {
            _parsedPages = new HashSet<string>();
            _deadLinks = new ObservableCollection<LinkInfo>();
            _pagesToParse = new HashSet<Link>(new LinkEqualityComparer());

            Result = "";
            UpdateStatusInfo();
        }
        private async void OnParse(string obj)
        {
            Init();

            if (string.IsNullOrWhiteSpace(Url))
            {
                var dialog = new MessageDialog("Please enter a url (with 'http://...')", pageTitle);
                await dialog.ShowAsync();
                return;
            }

            CanParse = false;
            CanCancel = true;

            try
            {
                List<Task<LinkInfo>> tasks = new List<Task<LinkInfo>>();
                _pagesToParse.Add(new Link(Url, Url));

                while (_pagesToParse.Count > 0 || tasks.Count() > 0)
                {
                    while (_pagesToParse.Count > 0 && tasks.Count < Speed)
                    {
                        if (CanCancel == false)
                        {
                            break;
                        }
                        Link link = _pagesToParse.First();
                        _pagesToParse.Remove(link);
                        _parsedPages.Add(link.Url);

                        Task<LinkInfo> newTask = ProceedPage(link);
                        tasks.Add(newTask);
                        //Result = $"{url} started\n"+Result;
                    }

                    if (CanCancel == false)
                    {
                        NotifyPropertyChanged("IsCancelling");
                        Result = "...cancelling\n" + Result;
                        break;
                    }

                    Task<LinkInfo> completedTask = await Task.WhenAny(tasks.ToArray());
                    //Result = completedTask.Result.ToString() + Result;

                    tasks = tasks.Where(i => i.Status != TaskStatus.RanToCompletion).ToList();

                    UpdateStatusInfo();
                }

                await Task.WhenAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                Result = ex.Message + "\n" + Result;
                CanParse = true;
            }


            UpdateStatusInfo();

            string message;
            if (CanCancel)
                message = $"Task completed.\nBroken links count: {BadLinks.Count}";
            else
            {
                Result = "Cancelled\n" + Result;
                message = "Task cancelled";
            }

            await new MessageDialog(message, pageTitle).ShowAsync();

            CanParse = true;
            CanCancel = false;
        }

        private void UpdateStatusInfo()
        {
            NotifyPropertyChanged("CheckedCount");
            NotifyPropertyChanged("InProcessCount");
            NotifyPropertyChanged("DeadLinksCount");
            NotifyPropertyChanged("BadLinks");
        }

        private async Task<LinkInfo> ProceedPage(Link host)
        {
            Parser parser = new Parser(host.Url);
            LinkInfo linkInfo = new LinkInfo(host);
            linkInfo.Status = await parser.GetPageStatus();
            if (linkInfo.Status != "OK")
            {
                _deadLinks.Add(new LinkInfo(host.Location, host.Url, linkInfo.Status));
                NotifyPropertyChanged("BadLinks");
            }

            HashSet<string> newLinks = await parser.FindLinks();
            if (newLinks != null)
            {
                lock (writeLock)
                {
                    foreach (string url in newLinks)
                    {
                        if (!_parsedPages.Contains(url))
                        {
                            _pagesToParse.Add(new Link(host.Url, url));
                        }
                        else
                        {
                            LinkInfo brokenLink = _deadLinks.Where(l => l.Url == url).FirstOrDefault();
                            if (brokenLink != null)
                            {
                                _deadLinks.Add(new LinkInfo(host.Url, url, brokenLink.Status));
                                NotifyPropertyChanged("BadLinks");
                            }
                        }
                    }
                }
            }


            return linkInfo;
        }


        private bool _canCancel;
        public bool CanCancel
        {
            get
            {
                return _canCancel;
            }
            set
            {
                _canCancel = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("IsCancelling");
            }
        }

        private void OnCancel(string obj)
        {
            CanCancel = false;
        }


        private string _url;
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand Parse { get; private set; }
        public ICommand Cancel { get; private set; }

        private string _result;
        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                NotifyPropertyChanged();
            }
        }

        //private string _badLinks;
        public ObservableCollection<LinkInfo> BadLinks
        {
            get
            {
                return _deadLinks;
            }
        }

        public string CheckedCount => _parsedPages.Count().ToString();
        public string InProcessCount => _pagesToParse.Count().ToString();
        public string DeadLinksCount => _deadLinks.Count().ToString();
        public bool IsCancelling => !CanCancel && !CanParse;

        private int _parsingSpeed;
        public int Speed
        {
            get
            {
                return _parsingSpeed;
            }
            set
            {
                _parsingSpeed = value;
                NotifyPropertyChanged();
            }
        }
    }
}
