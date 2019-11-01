using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ScrobbleMe
{
    partial class MainPage
    {
        //Update and read songs
        void ScrobbleRefresh_Click(object sender, RoutedEventArgs e)
        {
            Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
            ThreadLoadScrobble.Start();
        }

        //Update and add Scrobble songs
        void LoadScrobbleIgnore()
        {
            try
            {
                ProgressDisableUI("Refreshing played songs...");
                List<TblSong> DBSongQueryList = DBCon.TblSong.ToList();
                foreach (Song DevSong in vMediaLibrary.Songs.ToList().Where(x => x.PlayCount > 0).ToList())
                {
                    //Check for new or existing songs
                    TblSong DBSongItem = DBSongQueryList.Where(x => x.Artist == DevSong.Artist.Name & x.Album == DevSong.Album.Name & x.Title == DevSong.Name & x.Track == DevSong.TrackNumber & x.Genre == DevSong.Genre.Name & x.Duration == DevSong.Duration.ToString()).FirstOrDefault();
                    if (DBSongItem == null)
                    {
                        if (!String.IsNullOrWhiteSpace(DevSong.Artist.Name) && !String.IsNullOrWhiteSpace(DevSong.Album.Name) && !String.IsNullOrWhiteSpace(DevSong.Name))
                        {
                            TblSong AddSong = new TblSong();
                            AddSong.Artist = DevSong.Artist.Name;
                            AddSong.Album = DevSong.Album.Name;
                            AddSong.Title = DevSong.Name;
                            AddSong.Track = DevSong.TrackNumber;
                            AddSong.Genre = DevSong.Genre.Name;
                            AddSong.Duration = DevSong.Duration.ToString();
                            AddSong.Plays = DevSong.PlayCount;
                            DBCon.TblSong.InsertOnSubmit(AddSong);
                        }
                    }
                    else
                    {
                        //Update the songs playcount
                        if (DBSongItem.Plays != DevSong.PlayCount) { DBSongItem.Plays = DevSong.PlayCount; }

                        //Scrobble count above play check/fix
                        if (DBSongItem.Scrobbles > DevSong.PlayCount)
                        {
                            DBSongItem.Scrobbles = DevSong.PlayCount;
                            DBSongItem.Plays = DevSong.PlayCount;
                        }
                    }
                }
                DBCon.SubmitChanges();
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to update all your played songs.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); });
                ProgressEnableUI();
                return;
            }

            //Read and list Scrobble songs
            try
            {
                ProgressDisableUI("Loading played songs...");
                Dispatcher.BeginInvoke(delegate { lb_ScrobbleListBox.Items.Clear(); });

                List<TblSong> DBSongQueryList = DBCon.TblSong.ToList().Where(x => x.Plays > 0 & x.Plays > x.Scrobbles & x.Ignored == 0).OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Track).ToList();
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

                Dispatcher.BeginInvoke(delegate
                {
                    foreach (TblSong DBSong in DBSongQueryList.Take(200))
                    {
                        string TrackNumber = "";
                        if (DBSong.Track != 0) { TrackNumber = "|" + DBSong.Track; }
                        lb_ScrobbleListBox.Items.Add(new ScrobbleSongsList() { Id = DBSong.Id, Stats = TrackNumber + "|P" + DBSong.Plays + "/S" + DBSong.Scrobbles + "|", Artist = DBSong.Artist, Title = DBSong.Title, Album = DBSong.Album });
                    }

                    ShellTile ShellTile = ShellTile.ActiveTiles.FirstOrDefault();
                    if (DBSongQueryList.Count == 0)
                    {
                        txt_ScrobbleStats.Text = "Total songs found on this phone: " + vMediaLibrary.Songs.Count.ToString() + "\nLatest scrobble: " + vApplicationSettings["LastfmScrobbled"].ToString() + "\n\nThere were no songs found to scrobble, now is a great time to play some new songs to scrobble.\n\nIf you are using Windows Phone 8 you can use the 'Xbox Music' player to play songs to scrobble.\n\nWhen you are using Windows Mobile 10 'Groove Music' is sadly enough not supported so you will have to use a third party music player like 'Find My Music Too' or 'Finsic Music Player‏'";
                        lb_ScrobbleListBox.Visibility = Visibility.Collapsed;

                        if (ShellTile != null)
                        {
                            vStandardTileData.Title = "";
                            ShellTile.Update(vStandardTileData);
                        }
                    }
                    else
                    {
                        txt_ScrobbleStats.Text = "Total songs found on this phone: " + vMediaLibrary.Songs.Count.ToString() + "\nPlayed songs to scrobble to Last.fm: " + DBSongQueryList.Count.ToString() + "\nLatest scrobble: " + vApplicationSettings["LastfmScrobbled"].ToString();
                        //if (lb_ScrobbleListBox.Items.Count > 0) { lb_ScrobbleListBox.ScrollIntoView(lb_ScrobbleListBox.Items[0]); }
                        lb_ScrobbleListBox.Visibility = Visibility.Visible;

                        if ((bool)vApplicationSettings["ScrobbleTileCount"] && ShellTile != null)
                        {
                            vStandardTileData.Title = DBSongQueryList.Count.ToString();
                            ShellTile.Update(vStandardTileData);
                        }
                    }
                });
            }
            catch (Exception ex)
            { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to refresh all your played songs.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }); }

            //Read and list Ignored songs
            try
            {
                ProgressDisableUI("Loading ignored songs...");
                Dispatcher.BeginInvoke(delegate { lb_IgnoreListBox.Items.Clear(); });

                List<TblSong> DBIgnoreQueryList = DBCon.TblSong.ToList().Where(x => x.Ignored == 1).OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Track).ToList();
                //Sort songs to ignore
                if (!(bool)vApplicationSettings["LastfmScrobbleArtistOrder"])
                {
                    if ((bool)vApplicationSettings["LastfmScrobbleSongOrder"])
                    { DBIgnoreQueryList = DBIgnoreQueryList.OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Title).ToList(); }
                    else { DBIgnoreQueryList = DBIgnoreQueryList.OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Track).ToList(); }
                }
                else
                {
                    if ((bool)vApplicationSettings["LastfmScrobbleSongOrder"])
                    { DBIgnoreQueryList = DBIgnoreQueryList.OrderByDescending(x => x.Artist).ThenByDescending(x => x.Album).ThenByDescending(x => x.Title).ToList(); }
                    else { DBIgnoreQueryList = DBIgnoreQueryList.OrderByDescending(x => x.Artist).ThenByDescending(x => x.Album).ThenByDescending(x => x.Track).ToList(); }
                }

                Dispatcher.BeginInvoke(delegate
                {
                    foreach (TblSong DBSong in DBIgnoreQueryList)
                    {
                        string TrackNumber = "";
                        if (DBSong.Track != 0) { TrackNumber = "|" + DBSong.Track; }
                        lb_IgnoreListBox.Items.Add(new IgnoredSongsList() { Id = DBSong.Id, Stats = TrackNumber + "|P" + DBSong.Plays + "/S" + DBSong.Scrobbles + "|", Artist = DBSong.Artist, Title = DBSong.Title, Album = DBSong.Album });
                    }

                    if (DBIgnoreQueryList.Count == 0)
                    {
                        txt_IgnoreStats.Text = "There were no ignored songs found, if you don't want some songs to scrobble you can ignore them on the scrobble tab by selecting songs and click on the 'Ignore' button.";
                        lb_IgnoreListBox.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txt_IgnoreStats.Text = "Total ignored songs found on this phone: " + DBIgnoreQueryList.Count.ToString() + "\nTap on the songs that you want to unignore:";
                        //if (lb_IgnoreListBox.Items.Count > 0) { lb_IgnoreListBox.ScrollIntoView(lb_IgnoreListBox.Items[0]); }
                        lb_IgnoreListBox.Visibility = Visibility.Visible;
                    }
                });
            }
            catch (Exception ex)
            { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to refresh all your ignored songs.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }); }

            ProgressEnableUI();
            Dispatcher.BeginInvoke(delegate { HideLoadSplashScreen(); });
            return;
        }

        //ScrobbleSongsList Databinding
        public class ScrobbleSongsList
        {
            public int Id { get; set; }
            public string Stats { get; set; }
            public string Artist { get; set; }
            public string Title { get; set; }
            public string Album { get; set; }
        }

        //IgnoredSongsList Databinding
        public class IgnoredSongsList
        {
            public int Id { get; set; }
            public string Stats { get; set; }
            public string Artist { get; set; }
            public string Title { get; set; }
            public string Album { get; set; }
        }
    }
}