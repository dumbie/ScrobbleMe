using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Windows.Web.Http;

namespace ScrobbleMe
{
    partial class MainPage
    {
        //Refresh recently scrobbled button
        async void RecentRefresh_Click(object sender, RoutedEventArgs e)
        {
            //Check account name and password
            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
            {
                MessageBox.Show("Please set your Last.fm account name and password in the settings tab and login to view your recently scrobbled songs.", "ScrobbleMe", MessageBoxButton.OK);
                lb_RecentListBox.Items.Clear();
                MainPivot.SelectedItem = Settings;
                return;
            }
            await LoadRecent();
            return;
        }

        //Load recently scrobbled songs
        async Task LoadRecent()
        {
            //Check account name and password
            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
            {
                Dispatcher.BeginInvoke(delegate
                {
                    txt_RecentStats.Text = "Please set your Last.fm account name and password in the settings tab and login to view your recently scrobbled songs.";
                    lb_RecentListBox.Items.Clear();
                });
                return;
            }

            //Connect to last.fm and get recently scrobbled songs
            try
            {
                ProgressDisableUI("Refreshing recent scrobbles...");
                Dispatcher.BeginInvoke(delegate { lb_RecentListBox.Items.Clear(); });
                XDocument RecentXDoc = null;

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Dispatcher.BeginInvoke(delegate { txt_RecentOffline.Visibility = Visibility.Collapsed; });
                    using (HttpClient HttpClientRecent = new HttpClient())
                    {
                        HttpClientRecent.DefaultRequestHeaders.Add("User-Agent", "ScrobbleMe");
                        HttpClientRecent.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
                        HttpClientRecent.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store");
                        //Yes, I know I didn't remove the api key.
                        RecentXDoc = XDocument.Parse(await HttpClientRecent.GetStringAsync(new Uri("https://ws.audioscrobbler.com/2.0/?api_key=a62159e276986acf81f6990148b06ae3&method=user.getRecentTracks&user=" + HttpUtility.UrlEncode(vApplicationSettings["LastfmAccount"].ToString()) + "&limit=30&nc=" + Environment.TickCount)));
                    }

                    //Save Data to XML
                    using (IsolatedStorageFileStream IsolatedStorageFileStream = IsolatedStorageFile.GetUserStoreForApplication().CreateFile("RecentData.xml"))
                    { using (StreamWriter StreamWriter = new StreamWriter(IsolatedStorageFileStream)) { StreamWriter.WriteLine(RecentXDoc); } }
                }
                else
                {
                    using (IsolatedStorageFile IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (IsolatedStorageFile.FileExists("RecentData.xml"))
                        {
                            Dispatcher.BeginInvoke(delegate { txt_RecentOffline.Visibility = Visibility.Visible; });
                            using (IsolatedStorageFileStream IsolatedStorageFileStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("RecentData.xml", FileMode.Open))
                            { RecentXDoc = XDocument.Load(IsolatedStorageFileStream); }
                        }
                    }
                }

                //Check if there is any content in xml
                if (RecentXDoc == null) { txt_RecentStats.Text = "It seems like there is no recent scrobbles data available, please refresh the recent scrobbles when you have an internet connection available."; }
                else
                {
                    foreach (XElement Info in RecentXDoc.Descendants("track"))
                    {
                        string Artist = Info.Element("artist").Value;
                        string Title = Info.Element("name").Value;
                        string Image = "/Assets/NoCover.jpg";
                        string Date = "Not available";

                        if (Info.Element("date") != null)
                        {
                            DateTime DTDate = DateTime.Parse(Info.Element("date").Value).ToLocalTime();
                            Date = DTDate.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("en-US")) + ", " + DTDate.ToShortTimeString();
                        }
                        else
                        { Date = "Listening now"; }

                        if ((bool)vApplicationSettings["LastfmDownloadCovers"]) { if (!String.IsNullOrEmpty(Info.Element("image").Value) && Info.Element("image").Value.StartsWith("http")) { Image = Info.Element("image").Value.Replace("/34s/", "/64s/"); } }
                        Dispatcher.BeginInvoke(delegate { lb_RecentListBox.Items.Add(new RecentlySongsList() { Artist = Artist, Title = Title, Date = Date, Image = Image }); });
                    }

                    Dispatcher.BeginInvoke(delegate
                    {
                        if (lb_RecentListBox.Items.Count == 0)
                        {
                            lb_RecentListBox.Visibility = Visibility.Collapsed;
                            txt_RecentStats.Text = "It seems like you haven't played any songs yet, now it's time to start playing some songs to scrobble!";
                        }
                        else
                        {
                            foreach (XElement Info in RecentXDoc.Descendants("recenttracks")) { txt_RecentStats.Text = "Total scrobbles on your Last.fm profile: " + Info.Attribute("total").Value + "\nYou have recently scrobbled the following songs:"; }
                            //if (lb_RecentListBox.Items.Count > 0) { lb_RecentListBox.ScrollIntoView(lb_RecentListBox.Items[0]); }
                            lb_RecentListBox.Visibility = Visibility.Visible;
                        }
                    });
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(delegate
                {
                    lb_RecentListBox.Visibility = Visibility.Collapsed;
                    txt_RecentStats.Text = "Failed to connect to Last.fm, please check your Last.fm account settings, try to relog into your account or check your internet connection.\n\nIt could also be that the requested data is not available at Last.fm, please try again later on to see if the requested data is available for you.";
                });
            }

            ProgressEnableUI();
            return;
        }

        //RecentlySongsList Databinding
        public class RecentlySongsList
        {
            public string Artist { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
            public string Image { get; set; }
        }
    }
}