//using Md5Encryption;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.IsolatedStorage;
//using System.Linq;
//using System.Net;
//using System.Net.NetworkInformation;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Xml.Linq;
//using Windows.Web.Http;

//namespace ScrobbleMe
//{
//    partial class MainPage
//    {
//        //Open recommended artist page
//        void LikeArtist_Tap(object sender, System.Windows.Input.GestureEventArgs e)
//        {
//            foreach (object SelItem in lb_LikeListBox.SelectedItems)
//            {
//                LikeList SelectedItem = (LikeList)SelItem;
//                MessageBoxResult MessageBoxButton_OkCancel = MessageBoxResult.OK;
//                if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { MessageBoxButton_OkCancel = MessageBox.Show("Do you want to open the '" + SelectedItem.Artist + "' Last.fm artist page in the web browser?", "ScrobbleMe", MessageBoxButton.OKCancel); }
//                if (MessageBoxButton_OkCancel == MessageBoxResult.OK)
//                {
//                    vWebBrowserTask.Uri = new Uri(SelectedItem.Url);
//                    vWebBrowserTask.Show();
//                }
//            }
//            return;
//        }

//        //Refresh recommended artists button
//        async void LikeRefresh_Click(object sender, RoutedEventArgs e)
//        {
//            //Check account name and password
//            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
//            {
//                MessageBox.Show("Please set your Last.fm account name and password in the settings tab and login to view your recommended artists.", "ScrobbleMe", MessageBoxButton.OK);
//                lb_LikeListBox.Items.Clear();
//                MainPivot.SelectedItem = Settings;
//                return;
//            }
//            await LoadLikes();
//            return;
//        }

//        //Load recommended artists
//        async Task LoadLikes()
//        {
//            //Check account name and password
//            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
//            {
//                Dispatcher.BeginInvoke(delegate
//                {
//                    txt_LikeStats.Text = "Please set your Last.fm account name and password in the settings tab and login to view your recommended artists.";
//                    lb_LikeListBox.Items.Clear();
//                });
//                return;
//            }

//            //Connect to last.fm and get recommended artists
//            try
//            {
//                ProgressDisableUI("Refreshing recommended artists...");
//                Dispatcher.BeginInvoke(delegate { lb_LikeListBox.Items.Clear(); });
//                List<TblSong> DBSongQueryList = DBCon.TblSong.ToList();
//                XDocument LikesXDoc = null;

//                if (NetworkInterface.GetIsNetworkAvailable())
//                {
//                    Dispatcher.BeginInvoke(delegate { txt_LikeOffline.Visibility = Visibility.Collapsed; });

//                    string NoCache = Environment.TickCount.ToString();
//                    string LastFMApiKey = "a62159e276986acf81f6990148b06ae3"; //Yes, I know I didn't remove the api key.
//                    string LastFMApiSecret = "fa570ce8eeb81a3e1685b0e8a27d6517"; //Yes, I know I didn't remove the api key.
//                    string LastFMMethod = "user.getRecommendedArtists";
//                    string LastFMAuthToken = MD5CryptoServiceProvider.GetMd5String(vApplicationSettings["LastfmAccount"].ToString().ToLower() + vApplicationSettings["LastfmPassword"].ToString());
//                    string LastFMApiSig = MD5CryptoServiceProvider.GetMd5String("api_key" + LastFMApiKey + "authToken" + LastFMAuthToken + "limit20" + "method" + LastFMMethod + "nc" + NoCache + "sk" + vApplicationSettings["LastfmSessionKey"].ToString() + LastFMApiSecret);

//                    using (HttpClient HttpClientLikes = new HttpClient())
//                    {
//                        HttpClientLikes.DefaultRequestHeaders.Add("User-Agent", "ScrobbleMe");
//                        HttpClientLikes.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
//                        HttpClientLikes.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store");
//                        LikesXDoc = XDocument.Parse(await HttpClientLikes.GetStringAsync(new Uri("https://ws.audioscrobbler.com/2.0/?api_key=" + HttpUtility.UrlEncode(LastFMApiKey) + "&authToken=" + LastFMAuthToken + "&limit=20" + "&method=" + HttpUtility.UrlEncode(LastFMMethod) + "&nc=" + NoCache + "&sk=" + HttpUtility.UrlEncode(vApplicationSettings["LastfmSessionKey"].ToString()) + "&api_sig=" + HttpUtility.UrlEncode(LastFMApiSig))));
//                    }

//                    //Save Data to XML
//                    using (IsolatedStorageFileStream IsolatedStorageFileStream = IsolatedStorageFile.GetUserStoreForApplication().CreateFile("LikesData.xml"))
//                    { using (StreamWriter StreamWriter = new StreamWriter(IsolatedStorageFileStream)) { StreamWriter.WriteLine(LikesXDoc); } }
//                }
//                else
//                {
//                    using (IsolatedStorageFile IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
//                    {
//                        if (IsolatedStorageFile.FileExists("LikesData.xml"))
//                        {
//                            Dispatcher.BeginInvoke(delegate { txt_LikeOffline.Visibility = Visibility.Visible; });
//                            using (IsolatedStorageFileStream IsolatedStorageFileStream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("LikesData.xml", FileMode.Open))
//                            { LikesXDoc = XDocument.Load(IsolatedStorageFileStream); }
//                        }
//                    }
//                }

//                //Check if there is any content in xml
//                if (LikesXDoc == null) { txt_LikeStats.Text = "It seems like there is no recommended artists data available, please refresh the recommended artists when you have an internet connection available."; }
//                else
//                {
//                    foreach (XElement Info in LikesXDoc.Descendants("artist"))
//                    {
//                        string Artist = Info.Element("name").Value;
//                        string Url = Info.Element("url").Value;
//                        string Image = "/Assets/NoCover.jpg";

//                        if ((bool)vApplicationSettings["LastfmDownloadCovers"]) { if (!String.IsNullOrEmpty(Info.Element("image").Value) && Info.Element("image").Value.StartsWith("http")) { Image = Info.Element("image").Value.Replace("/34s/", "/64s/"); } }
//                        Dispatcher.BeginInvoke(delegate { if (!lb_LikeListBox.Items.Any(x => ((LikeList)x).Url == Url) && !DBSongQueryList.Any(x => x.Artist == Artist)) { lb_LikeListBox.Items.Add(new LikeList() { Artist = Artist, Url = Url, Image = Image }); } });
//                    }

//                    Dispatcher.BeginInvoke(delegate
//                    {
//                        if (lb_LikeListBox.Items.Count == 0)
//                        {
//                            lb_LikeListBox.Visibility = Visibility.Collapsed;
//                            txt_LikeStats.Text = "It seems like you haven't got a recommendation, it's time to start playing some songs to check out your recommended artists and discover some great new music to listen to.";
//                        }
//                        else
//                        {
//                            txt_LikeStats.Text = "Check out those artists that you might like based on your all time Last.fm profile scrobbles:";
//                            //if (lb_LikeListBox.Items.Count > 0) { lb_LikeListBox.ScrollIntoView(lb_LikeListBox.Items[0]); }
//                            lb_LikeListBox.Visibility = Visibility.Visible;
//                        }
//                    });
//                }
//            }
//            catch
//            {
//                Dispatcher.BeginInvoke(delegate
//                {
//                    lb_LikeListBox.Visibility = Visibility.Collapsed;
//                    txt_LikeStats.Text = "Failed to connect to Last.fm, please check your Last.fm account settings, try to relog into your account or check your internet connection.\n\nIt could also be that the requested data is not available at Last.fm, please try again later on to see if the requested data is available for you.";
//                });
//            }

//            ProgressEnableUI();
//            return;
//        }

//        //LikeList Databinding
//        public class LikeList
//        {
//            public string Artist { get; set; }
//            public string Url { get; set; }
//            public string Image { get; set; }
//        }
//    }
//}