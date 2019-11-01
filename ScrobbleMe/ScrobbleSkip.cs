using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ScrobbleMe
{
    partial class MainPage
    {
        //Skip Certain Songs button
        void SkipCertSongs_Click(object sender, RoutedEventArgs e)
        {
            //Check selected songs
            if (lb_ScrobbleListBox.SelectedItems.Count == 0)
            {
                if (!(bool)vApplicationSettings["LastfmReduceConfirmation"])
                { MessageBox.Show("There were no songs selected to skip, please select some songs from the list to skip first.", "ScrobbleMe", MessageBoxButton.OK); }
                return;
            }

            //Skip confirmation
            MessageBoxResult MessageBoxButton_OkCancel = MessageBoxResult.OK;
            if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { MessageBoxButton_OkCancel = MessageBox.Show("Do you want to skip the selected songs? You won't be able to recover the plays!", "ScrobbleMe", MessageBoxButton.OKCancel); }
            if (MessageBoxButton_OkCancel == MessageBoxResult.OK)
            {
                Thread ThreadSkipCert = new Thread(() => SkipCertSongs());
                ThreadSkipCert.Start();
            }
            return;
        }

        void SkipCertSongs()
        {
            try
            {
                ProgressDisableUI("Skipping selected played songs...");
                List<TblSong> DBSongQueryList = DBCon.TblSong.ToList();
                foreach (object SelItem in lb_ScrobbleListBox.SelectedItems)
                {
                    ScrobbleSongsList SelectedItem = (ScrobbleSongsList)SelItem;
                    TblSong DBSong = DBSongQueryList.Where(x => x.Id == SelectedItem.Id).FirstOrDefault();
                    DBSong.Scrobbles = DBSong.Plays;
                }
                DBCon.SubmitChanges();

                if (!(bool)vApplicationSettings["LastfmReduceConfirmation"])
                { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Your selected played songs have been skipped, it's time to scrobble the songs!", "ScrobbleMe", MessageBoxButton.OK); }); }

                Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
                ThreadLoadScrobble.Start();
                return;
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to skip your selected played songs.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); });
                Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
                ThreadLoadScrobble.Start();
                return;
            }
        }

        //Skip All Songs button
        void SkipAllSongs_Click(object sender, RoutedEventArgs e)
        {
            //Skip confirmation
            MessageBoxResult MessageBoxButton_OkCancel = MessageBoxResult.OK;
            MessageBoxButton_OkCancel = MessageBox.Show("Do you want to skip all the played songs? You won't be able to recover the plays!", "ScrobbleMe", MessageBoxButton.OKCancel);
            if (MessageBoxButton_OkCancel == MessageBoxResult.OK)
            {
                Thread ThreadSkipAll = new Thread(() => SkipAllSongs());
                ThreadSkipAll.Start();
            }
            return;
        }

        void SkipAllSongs()
        {
            try
            {
                ProgressDisableUI("Skipping all played songs...");
                List<TblSong> DBSongQueryList = DBCon.TblSong.ToList().Where(x => x.Plays > 0 & x.Plays > x.Scrobbles & x.Ignored == 0).ToList();
                foreach (TblSong DBSong in DBSongQueryList) { DBSong.Scrobbles = DBSong.Plays; }
                DBCon.SubmitChanges();

                if (!(bool)vApplicationSettings["LastfmReduceConfirmation"])
                { Dispatcher.BeginInvoke(delegate { MessageBox.Show("All your played songs have been skipped, now it's time to play some new songs!", "ScrobbleMe", MessageBoxButton.OK); }); }
            }
            catch (Exception ex)
            { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to skip all your played songs.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }); }

            Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
            ThreadLoadScrobble.Start();
            return;
        }
    }
}
