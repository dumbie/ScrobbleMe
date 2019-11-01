using Md5Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using Windows.Web.Http;

namespace ScrobbleMe
{
    partial class MainPage
    {
        //Scrobble Songs to Last.fm
        void ScrobbleSongs_Click(object sender, RoutedEventArgs e)
        {
            //Check account name and password
            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()))
            {
                MessageBox.Show("Please set your Last.fm account name and password in the settings tab and login to start scrobbling to your Last.fm profile.", "ScrobbleMe", MessageBoxButton.OK);
                MainPivot.SelectedItem = Settings;
                return;
            }

            //Check if there is an internet connection
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("It seems like you currently don't have an internet connection available, please make sure you have an internet connection before you try to scrobble.", "ScrobbleMe", MessageBoxButton.OK);
                return;
            }

            //Scrobble Confirmation
            MessageBoxResult MessageBoxButton_OkCancel = MessageBoxResult.OK;
            if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { MessageBoxButton_OkCancel = MessageBox.Show("Do you want to start scrobbling all your played songs to your Last.fm account?", "ScrobbleMe", MessageBoxButton.OKCancel); }
            if (MessageBoxButton_OkCancel == MessageBoxResult.OK)
            {
                Thread ThreadScrobbleSongs = new Thread(() => ScrobbleSongs());
                ThreadScrobbleSongs.Start();
            }
            return;
        }

        async void ScrobbleSongs()
        {
            try
            {
                //Check for songs to scrobble
                ProgressDisableUI("Checking scrobble songs...");
                List<TblSong> DBSongQueryList = DBCon.TblSong.ToList().Where(x => x.Plays > 0 & x.Plays > x.Scrobbles & x.Ignored == 0).ToList();

                //Check if there are songs to scrobble
                if (DBSongQueryList.Count == 0)
                {
                    if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { Dispatcher.BeginInvoke(delegate { MessageBox.Show("There were no songs found to scrobble, now it's time to play some new songs!", "ScrobbleMe", MessageBoxButton.OK); }); }
                    ProgressEnableUI();
                    return;
                }

                //Sort songs to scrobble
                if (!(bool)vApplicationSettings["LastfmScrobbleArtistOrder"])
                {
                    if ((bool)vApplicationSettings["LastfmScrobbleSongOrder"]) { DBSongQueryList = DBSongQueryList.OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Title).ToList(); }
                    else { DBSongQueryList = DBSongQueryList.OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Track).ToList(); }
                }
                else
                {
                    if ((bool)vApplicationSettings["LastfmScrobbleSongOrder"]) { DBSongQueryList = DBSongQueryList.OrderByDescending(x => x.Artist).ThenByDescending(x => x.Album).ThenByDescending(x => x.Title).ToList(); }
                    else { DBSongQueryList = DBSongQueryList.OrderByDescending(x => x.Artist).ThenByDescending(x => x.Album).ThenByDescending(x => x.Track).ToList(); }
                }

                //Last.fm scrobble song
                int SubmitCountTotal = 0;
                int SubmitCountBatch = 0;
                int SubmitCountMax = 0;

                string SubmitResult = "";
                string SubmitErrorMsg = "";
                DateTime SubmitUnixTime = DateTime.UtcNow.AddMinutes(-(int)vApplicationSettings["LastfmDelayTimeInt"]);

                //Submit to Last.fm #1
                string LastFMApiKey = "a62159e276986acf81f6990148b06ae3"; //Yes, I know I didn't remove the api key.
                string LastFMApiSecret = "fa570ce8eeb81a3e1685b0e8a27d6517"; //Yes, I know I didn't remove the api key.
                string LastFMMethod = "track.scrobble";

                List<int> ScrobbleBatchListId = new List<int>();
                Dictionary<string, string> ScrobbleBatchList = new Dictionary<string, string>();
                ScrobbleBatchList.Add("api_key", LastFMApiKey);
                ScrobbleBatchList.Add("method", LastFMMethod);
                ScrobbleBatchList.Add("sk", vApplicationSettings["LastfmSessionKey"].ToString());

                //Calculate maximum songs
                foreach (TblSong MaxDBSong in DBSongQueryList)
                {
                    int ScrobbleCount = MaxDBSong.Plays - MaxDBSong.Scrobbles;
                    if ((bool)vApplicationSettings["SkipMultiplePlays"]) { ScrobbleCount = 1; }
                    for (int i = 1; i <= ScrobbleCount; i++) { SubmitCountMax++; }
                }

                //Batch scrobble songs
                foreach (TblSong DBSong in DBSongQueryList)
                {
                    int ScrobbleCount = DBSong.Plays - DBSong.Scrobbles;
                    if ((bool)vApplicationSettings["SkipMultiplePlays"]) { ScrobbleCount = 1; }
                    for (int i = 1; i <= ScrobbleCount; i++)
                    {
                        //Calculate scrobble time
                        TimeSpan SongSeconds = new TimeSpan(Convert.ToDateTime(DBSong.Duration).Hour, Convert.ToDateTime(DBSong.Duration).Minute, Convert.ToDateTime(DBSong.Duration).Second);
                        SubmitUnixTime = SubmitUnixTime.AddSeconds(-SongSeconds.TotalSeconds);
                        long SubmitUnixTimeTicks = (SubmitUnixTime.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks) / 10000000;

                        ScrobbleBatchListId.Add(DBSong.Id);
                        ScrobbleBatchList.Add("album[" + SubmitCountBatch + "]", DBSong.Album);
                        ScrobbleBatchList.Add("artist[" + SubmitCountBatch + "]", DBSong.Artist);
                        ScrobbleBatchList.Add("duration[" + SubmitCountBatch + "]", Convert.ToString(SongSeconds.TotalSeconds));
                        ScrobbleBatchList.Add("timestamp[" + SubmitCountBatch + "]", Convert.ToString(SubmitUnixTimeTicks));
                        ScrobbleBatchList.Add("track[" + SubmitCountBatch + "]", DBSong.Title);
                        ScrobbleBatchList.Add("trackNumber[" + SubmitCountBatch + "]", Convert.ToString(DBSong.Track));

                        SubmitCountBatch++;
                        SubmitCountTotal++;

                        if (SubmitCountBatch == 15 || SubmitCountTotal == SubmitCountMax)
                        {
                            ProgressDisableUI("Scrobbling songs: " + SubmitCountTotal + "/" + SubmitCountMax);

                            List<string> ScrobbleBatchListSorted = ScrobbleBatchList.Select(x => x.Key).ToList();
                            ScrobbleBatchListSorted.Sort(StringComparer.Ordinal);

                            //Build Signature String
                            StringBuilder sbSig = new StringBuilder();
                            foreach (string Key in ScrobbleBatchListSorted)
                            { sbSig.Append(Key.ToString() + ScrobbleBatchList[Key]); }
                            sbSig.Append(LastFMApiSecret);

                            //Build Post String
                            StringBuilder sbPost = new StringBuilder();
                            foreach (string Key in ScrobbleBatchListSorted)
                            { sbPost.Append("&" + Key.ToString() + "=" + HttpUtility.UrlEncode(ScrobbleBatchList[Key])); }
                            sbPost.Append("&api_sig=" + HttpUtility.UrlEncode(MD5CryptoServiceProvider.GetMd5String(sbSig.ToString())));

                            XDocument ResponseXml = null;
                            using (HttpClient HttpClientSubmit = new HttpClient())
                            {
                                HttpClientSubmit.DefaultRequestHeaders.Add("User-Agent", "ScrobbleMe");
                                HttpClientSubmit.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
                                HttpClientSubmit.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store");

                                Uri PostUri = new Uri("https://ws.audioscrobbler.com/2.0/");
                                HttpStringContent PostContent = new HttpStringContent(sbPost.ToString(), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");

                                ResponseXml = XDocument.Parse((await HttpClientSubmit.PostAsync(PostUri, PostContent)).Content.ToString());
                            }

                            //Submit to Last.fm #2
                            if (ResponseXml.Element("lfm").Attribute("status").Value == "ok")
                            {
                                SubmitResult = "ok";

                                //Mark batch songs as scrobbled
                                foreach (int SongID in ScrobbleBatchListId)
                                {
                                    TblSong DBSongID = DBSongQueryList.Where(x => x.Id == SongID).FirstOrDefault();
                                    DBSongID.Scrobbles = DBSongID.Plays;
                                }
                                DBCon.SubmitChanges();

                                if (Convert.ToUInt32(ResponseXml.Element("lfm").Element("scrobbles").Attribute("ignored").Value) > 0)
                                { SubmitErrorMsg = " But some songs might have been ignored by Last.fm."; }
                            }
                            else if (!String.IsNullOrEmpty(ResponseXml.Element("lfm").Element("error").Value))
                            {
                                Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to scrobble your songs to Last.fm, please check your Last.fm account settings, try to relog into your account or check your internet connection.\n\nError Message: " + ResponseXml.Element("lfm").Element("error").Value, "ScrobbleMe", MessageBoxButton.OK); });
                                SubmitResult = "failed";
                                break;
                            }

                            SubmitCountBatch = 0;
                            ScrobbleBatchListId.Clear();
                            ScrobbleBatchList.Clear();
                            ScrobbleBatchList.Add("api_key", LastFMApiKey);
                            ScrobbleBatchList.Add("method", LastFMMethod);
                            ScrobbleBatchList.Add("sk", vApplicationSettings["LastfmSessionKey"].ToString());
                        }
                    };
                    if (SubmitResult == "failed") { break; }
                }

                if (SubmitResult == "ok")
                {
                    //Set last scrobble date
                    vApplicationSettings["LastfmScrobbled"] = DateTime.Now.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("en-US")) + " at " + DateTime.Now.ToShortTimeString();
                    vApplicationSettings.Save();

                    if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { Dispatcher.BeginInvoke(delegate { MessageBox.Show("All your played songs have successfully been scrobbled to your Last.fm profile." + SubmitErrorMsg, "ScrobbleMe", MessageBoxButton.OK); }); }
                }
            }
            catch (Exception ex)
            { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to scrobble your songs to Last.fm, please check your Last.fm account settings, try to relog into your account or check your internet connection.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }); }

            if ((bool)vApplicationSettings["LoadLastFMData"])
            {
                await LoadLoved();
                await LoadArtists();
                //await LoadLikes();
            }

            await LoadRecent();
            Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
            ThreadLoadScrobble.Start();
            return;
        }
    }
}