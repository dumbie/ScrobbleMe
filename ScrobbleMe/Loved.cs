using Md5Encryption;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Windows.Web.Http;

namespace ScrobbleMe
{
    partial class MainPage
    {
        //Refresh loved songs button
        async void LovedRefresh_Click(object sender, RoutedEventArgs e)
        {
            //Check account name and password
            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
            {
                MessageBox.Show("Please set your Last.fm account name and password in the settings tab and login to view your loved songs.", "ScrobbleMe", MessageBoxButton.OK);
                lb_LovedListBox.Items.Clear();
                MainPivot.SelectedItem = Settings;
                return;
            }
            await LoadLoved();
            return;
        }

        //Load loved songs
        async Task LoadLoved()
        {
            //Check account name and password
            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
            {
                Dispatcher.BeginInvoke(delegate
                {
                    txt_LovedStats.Text = "Please set your Last.fm account name and password in the settings tab and login to view your loved songs.";
                    lb_LovedListBox.Items.Clear();
                });
                return;
            }

            //Connect to last.fm and get loved songs
            try
            {
                ProgressDisableUI("Refreshing loved songs...");
                Dispatcher.BeginInvoke(delegate { lb_LovedListBox.Items.Clear(); });
                XDocument LovedXDoc = null;

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Dispatcher.BeginInvoke(delegate { txt_LovedOffline.Visibility = Visibility.Collapsed; });
                    using (HttpClient HttpClientLoved = new HttpClient())
                    {
                        HttpClientLoved.DefaultRequestHeaders.Add("User-Agent", "ScrobbleMe");
                        HttpClientLoved.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
                        HttpClientLoved.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store");
                        //Yes, I know I didn't remove the api key.
                        LovedXDoc = XDocument.Parse(await HttpClientLoved.GetStringAsync(new Uri("https://ws.audioscrobbler.com/2.0/?api_key=a62159e276986acf81f6990148b06ae3&method=user.getLovedTracks&user=" + HttpUtility.UrlEncode(vApplicationSettings["LastfmAccount"].ToString()) + "&limit=30&nc=" + Environment.TickCount)));
                    }

                    //Save Data to XML
                    using (IsolatedStorageFileStream IsolatedStorageFileStream = IsolatedStorageFile.GetUserStoreForApplication().CreateFile("LovedData.xml"))
                    { using (StreamWriter StreamWriter = new StreamWriter(IsolatedStorageFileStream)) { StreamWriter.WriteLine(LovedXDoc); } }
                }
                else
                {
                    using (IsolatedStorageFile IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (IsolatedStorageFile.FileExists("LovedData.xml"))
                        {
                            Dispatcher.BeginInvoke(delegate { txt_LovedOffline.Visibility = Visibility.Visible; });
                            using (IsolatedStorageFileStream IsolatedStorageFileStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("LovedData.xml", FileMode.Open))
                            { LovedXDoc = XDocument.Load(IsolatedStorageFileStream); }
                        }
                    }
                }

                //Check if there is any content in xml
                if (LovedXDoc == null) { txt_LovedStats.Text = "It seems like there is no loved songs data available, please refresh the loved songs when you have an internet connection available."; }
                else
                {
                    foreach (XElement Info in LovedXDoc.Descendants("track"))
                    {
                        string Artist = Info.Element("artist").Element("name").Value;
                        string Title = Info.Element("name").Value;
                        string Image = "/Assets/NoCover.jpg";
                        string Date = "Not available";

                        if (Info.Element("date") != null)
                        {
                            DateTime DTDate = DateTime.Parse(Info.Element("date").Value).ToLocalTime();
                            Date = DTDate.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("en-US")) + ", " + DTDate.ToShortTimeString();
                        }
                        else
                        { Date = "Recently loved"; }

                        if ((bool)vApplicationSettings["LastfmDownloadCovers"]) { if (Info.Element("image") != null && Info.Element("image").Value.StartsWith("http")) { Image = Info.Element("image").Value.Replace("/34s/", "/64s/"); } }
                        Dispatcher.BeginInvoke(delegate { lb_LovedListBox.Items.Add(new LovedSongsList() { Artist = Artist, Title = Title, Date = Date, Image = Image }); });
                    }

                    Dispatcher.BeginInvoke(delegate
                    {
                        if (lb_LovedListBox.Items.Count == 0)
                        {
                            lb_LovedListBox.Visibility = Visibility.Collapsed;
                            txt_LovedStats.Text = "It seems like you haven't loved any songs yet, it's time to start loving your music a bit more on your Last.fm profile <3\n\nYou can start loving your songs by synchronizing your loved songs from your device by clicking on the 'Sync loved songs' button.";
                        }
                        else
                        {
                            foreach (XElement Info in LovedXDoc.Descendants("lovedtracks")) { txt_LovedStats.Text = "Total loved songs on your Last.fm profile: " + Info.Attribute("total").Value + "\nYou have recently loved the following songs:"; }
                            //if (lb_LovedListBox.Items.Count > 0) { lb_LovedListBox.ScrollIntoView(lb_LovedListBox.Items[0]); }
                            lb_LovedListBox.Visibility = Visibility.Visible;
                        }
                    });
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(delegate
                {
                    lb_LovedListBox.Visibility = Visibility.Collapsed;
                    txt_LovedStats.Text = "Failed to connect to Last.fm, please check your Last.fm account settings, try to relog into your account or check your internet connection.\n\nIt could also be that the requested data is not available at Last.fm, please try again later on to see if the requested data is available for you.";
                });
            }

            ProgressEnableUI();
            return;
        }

        //Sync Loved button
        void LovedSync_Click(object sender, RoutedEventArgs e)
        {
            //Check account name and password
            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
            {
                MessageBox.Show("Please set your Last.fm account name and password in the settings tab and login to start syncing your loved songs.", "ScrobbleMe", MessageBoxButton.OK);
                MainPivot.SelectedItem = Settings;
                return;
            }

            //Check if there is an internet connection
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("It seems like you currently don't have an internet connection available, please make sure you have an internet connection before you try to sync your loved songs.", "ScrobbleMe", MessageBoxButton.OK);
                return;
            }

            //Sync confirmation
            MessageBoxResult MessageBoxButton_OkCancel = MessageBoxResult.OK;
            if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { MessageBoxButton_OkCancel = MessageBox.Show("Do you want to sync your device's loved songs with your set Last.fm profile?", "ScrobbleMe", MessageBoxButton.OKCancel); }
            if (MessageBoxButton_OkCancel == MessageBoxResult.OK)
            {
                Thread ThreadSyncLovedSongs = new Thread(() => SyncLovedSongs());
                ThreadSyncLovedSongs.Start();
            }
            return;
        }

        //Sync loved songs
        async void SyncLovedSongs()
        {
            try
            {
                //Check loved songs
                ProgressDisableUI("Checking for loved songs...");
                List<Song> MediaLibraryLovedList = vMediaLibrary.Songs.ToList().Where(x => x.Rating == 8).ToList();
                if (MediaLibraryLovedList.Count == 0)
                {
                    if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { Dispatcher.BeginInvoke(delegate { MessageBox.Show("It seems like you haven't loved any songs yet, it's time to start loving your music a bit more in your music player <3", "ScrobbleMe", MessageBoxButton.OK); }); }
                    ProgressEnableUI();
                    return;
                }

                //Submit loved songs
                int SubmitCount = 0;
                string SubmitResult = "";
                string SubmitErrorMsg = "";

                //Submit to Last.fm #1
                string LastFMApiKey = "a62159e276986acf81f6990148b06ae3"; //Yes, I know I didn't remove the api key.
                string LastFMApiSecret = "fa570ce8eeb81a3e1685b0e8a27d6517"; //Yes, I know I didn't remove the api key.
                string LastFMMethod = "track.love";

                foreach (Song DevSong in MediaLibraryLovedList.ToList())
                {
                    SubmitCount++;
                    ProgressDisableUI("|" + SubmitCount + "/" + MediaLibraryLovedList.Count() + "| " + DevSong.Artist + " - " + DevSong.Name + " (" + DevSong.Album + ")");

                    //Submit to Last.fm #2
                    string LastFMApiSig = MD5CryptoServiceProvider.GetMd5String("api_key" + LastFMApiKey + "artist" + DevSong.Artist.ToString() + "method" + LastFMMethod + "sk" + vApplicationSettings["LastfmSessionKey"].ToString() + "track" + DevSong.Name.ToString() + LastFMApiSecret);

                    XDocument ResponseXml = null;
                    using (HttpClient HttpClientLoved = new HttpClient())
                    {
                        HttpClientLoved.DefaultRequestHeaders.Add("User-Agent", "ScrobbleMe");
                        HttpClientLoved.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
                        HttpClientLoved.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store");

                        Uri PostUri = new Uri("https://ws.audioscrobbler.com/2.0/");
                        HttpStringContent PostContent = new HttpStringContent("api_key=" + HttpUtility.UrlEncode(LastFMApiKey) + "&artist=" + HttpUtility.UrlEncode(DevSong.Artist.ToString()) + "&method=" + HttpUtility.UrlEncode(LastFMMethod) + "&sk=" + HttpUtility.UrlEncode(vApplicationSettings["LastfmSessionKey"].ToString()) + "&track=" + HttpUtility.UrlEncode(DevSong.Name.ToString()) + "&api_sig=" + HttpUtility.UrlEncode(LastFMApiSig), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");

                        ResponseXml = XDocument.Parse((await HttpClientLoved.PostAsync(PostUri, PostContent)).Content.ToString());
                    }

                    if (ResponseXml.Element("lfm").Attribute("status").Value == "ok") { SubmitResult = "ok"; }
                    else if (!String.IsNullOrEmpty(ResponseXml.Element("lfm").Element("error").Value))
                    {
                        if (ResponseXml.Element("lfm").Element("error").Value.Contains("Track not recognised"))
                        {
                            SubmitErrorMsg = " But atleast one song was not recognized and loved.";
                            SubmitResult = "ok";
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to love your songs on Last.fm, please check your Last.fm account settings, try to relog into your account or check your internet connection.\n\nError Message: " + ResponseXml.Element("lfm").Element("error").Value, "ScrobbleMe", MessageBoxButton.OK); });
                            SubmitResult = "failed";
                            break;
                        }
                    }
                }
                if (SubmitResult == "ok") { if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Your loved songs have successfully been submitted to your Last.fm profile." + SubmitErrorMsg, "ScrobbleMe", MessageBoxButton.OK); }); } }
            }
            catch (Exception ex)
            { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to love your songs on Last.fm, please check your Last.fm account settings, try to relog into your account or check your internet connection.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }); }

            await LoadLoved();
            ProgressEnableUI();
            return;
        }

        //LovedSongsList Databinding
        public class LovedSongsList
        {
            public string Artist { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
            public string Image { get; set; }
        }
    }
}