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
        //Refresh top artists button
        async void ArtistsRefresh_Click(object sender, RoutedEventArgs e)
        {
            //Check account name and password
            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
            {
                MessageBox.Show("Please set your Last.fm account name and password in the settings tab and login to view your top artists.", "ScrobbleMe", MessageBoxButton.OK);
                lb_ArtistsListBox.Items.Clear();
                MainPivot.SelectedItem = Settings;
                return;
            }
            await LoadArtists();
            return;
        }

        //Load top artists
        async Task LoadArtists()
        {
            //Check account name and password
            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
            {
                Dispatcher.BeginInvoke(delegate
                {
                    txt_ArtistsStats.Text = "Please set your Last.fm account name and password in the settings tab and login to view your top artists.";
                    lb_ArtistsListBox.Items.Clear();
                });
                return;
            }

            //Connect to last.fm and get top artists
            try
            {
                ProgressDisableUI("Refreshing top artists...");
                Dispatcher.BeginInvoke(delegate { lb_ArtistsListBox.Items.Clear(); });
                XDocument ArtistsXDoc = null;

                //Check load period
                string LastfmArtistLoadPeriod = "";
                string LastfmArtistLoadPeriodSubmit = "";
                switch ((int)vApplicationSettings["LastfmArtistLoadPeriodInt"])
                {
                    case 0: { LastfmArtistLoadPeriodSubmit = "&period=overall"; LastfmArtistLoadPeriod = "all time"; break; }
                    case 1: { LastfmArtistLoadPeriodSubmit = "&period=7day"; LastfmArtistLoadPeriod = "7 days"; break; }
                    case 2: { LastfmArtistLoadPeriodSubmit = "&period=3month"; LastfmArtistLoadPeriod = "3 months"; break; }
                    case 3: { LastfmArtistLoadPeriodSubmit = "&period=6month"; LastfmArtistLoadPeriod = "6 months"; break; }
                    case 4: { LastfmArtistLoadPeriodSubmit = "&period=12month"; LastfmArtistLoadPeriod = "12 months"; break; }
                }

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Dispatcher.BeginInvoke(delegate { txt_ArtistsOffline.Visibility = Visibility.Collapsed; });
                    using (HttpClient HttpClientArtists = new HttpClient())
                    {
                        HttpClientArtists.DefaultRequestHeaders.Add("User-Agent", "ScrobbleMe");
                        HttpClientArtists.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
                        HttpClientArtists.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store");
                        //Yes, I know I didn't remove the api key.
                        ArtistsXDoc = XDocument.Parse(await HttpClientArtists.GetStringAsync(new Uri("https://ws.audioscrobbler.com/2.0/?api_key=a62159e276986acf81f6990148b06ae3&method=user.getTopArtists&user=" + HttpUtility.UrlEncode(vApplicationSettings["LastfmAccount"].ToString()) + LastfmArtistLoadPeriodSubmit + "&limit=30&nc=" + Environment.TickCount)));
                    }

                    //Save Data to XML
                    using (IsolatedStorageFileStream IsolatedStorageFileStream = IsolatedStorageFile.GetUserStoreForApplication().CreateFile("ArtistsData.xml"))
                    { using (StreamWriter StreamWriter = new StreamWriter(IsolatedStorageFileStream)) { StreamWriter.WriteLine(ArtistsXDoc); } }
                }
                else
                {
                    using (IsolatedStorageFile IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (IsolatedStorageFile.FileExists("ArtistsData.xml"))
                        {
                            Dispatcher.BeginInvoke(delegate { txt_ArtistsOffline.Visibility = Visibility.Visible; });
                            using (IsolatedStorageFileStream IsolatedStorageFileStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("ArtistsData.xml", FileMode.Open))
                            { ArtistsXDoc = XDocument.Load(IsolatedStorageFileStream); }
                        }
                    }
                }

                //Check if there is any content in xml
                if (ArtistsXDoc == null) { txt_ArtistsStats.Text = "It seems like there is no top artists data available, please refresh the top artists when you have an internet connection available."; }
                else
                {
                    foreach (XElement Info in ArtistsXDoc.Descendants("artist"))
                    {
                        string Artist = "#" + Info.Attribute("rank").Value + " " + Info.Element("name").Value;
                        string Plays = Info.Element("playcount").Value + " total plays";
                        string Image = "/Assets/NoCover.jpg";

                        if ((bool)vApplicationSettings["LastfmDownloadCovers"]) { if (!String.IsNullOrEmpty(Info.Element("image").Value) && Info.Element("image").Value.StartsWith("http")) { Image = Info.Element("image").Value.Replace("/34s/", "/64s/"); } }
                        Dispatcher.BeginInvoke(delegate { lb_ArtistsListBox.Items.Add(new ArtistsList() { Artist = Artist, Plays = Plays, Image = Image }); });
                    }

                    Dispatcher.BeginInvoke(delegate
                    {
                        if (lb_ArtistsListBox.Items.Count == 0)
                        {
                            lb_ArtistsListBox.Visibility = Visibility.Collapsed;
                            txt_ArtistsStats.Text = "It seems like you haven't got a top artist yet, it's time to start playing some songs to check out your " + LastfmArtistLoadPeriod + " top artists.";
                        }
                        else
                        {
                            foreach (XElement Info in ArtistsXDoc.Descendants("topartists")) { txt_ArtistsStats.Text = LastfmArtistLoadPeriod.Substring(0, 1).ToUpper() + LastfmArtistLoadPeriod.Substring(1) + " artists on your Last.fm profile: " + Info.Attribute("total").Value + "\nYour current " + LastfmArtistLoadPeriod + " played top artists are:"; }
                            //if (lb_ArtistsListBox.Items.Count > 0) { lb_ArtistsListBox.ScrollIntoView(lb_ArtistsListBox.Items[0]); }
                            lb_ArtistsListBox.Visibility = Visibility.Visible;
                        }
                    });
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(delegate
                {
                    lb_ArtistsListBox.Visibility = Visibility.Collapsed;
                    txt_ArtistsStats.Text = "Failed to connect to Last.fm, please check your Last.fm account settings, try to relog into your account or check your internet connection.\n\nIt could also be that the requested data is not available at Last.fm, please try again later on to see if the requested data is available for you.";
                });
            }

            ProgressEnableUI();
            return;
        }

        //ArtistsList Databinding
        public class ArtistsList
        {
            public string Artist { get; set; }
            public string Plays { get; set; }
            public string Image { get; set; }
        }
    }
}