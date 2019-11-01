using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ScrobbleMe
{
    partial class MainPage
    {
        //Ignore Certain Songs Function (Scrobble Page)
        void IgnoreCertSongs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Check selected songs
            if (lb_ScrobbleListBox.SelectedItems.Count == 0)
            {
                if (!(bool)vApplicationSettings["LastfmReduceConfirmation"])
                { MessageBox.Show("There were no songs selected to ignore, please select some songs from the list to ignore first.", "ScrobbleMe", MessageBoxButton.OK); }
                return;
            }

            MessageBoxResult MessageBoxButton_OkCancel = MessageBoxResult.OK;
            if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { MessageBoxButton_OkCancel = MessageBox.Show("Do you want to ignore the selected songs? You can easily unignore them later on from the ignore tab.", "ScrobbleMe", MessageBoxButton.OKCancel); }
            if (MessageBoxButton_OkCancel == MessageBoxResult.OK)
            {
                Thread ThreadIgnoreCert = new Thread(() => IgnoreCertSongs());
                ThreadIgnoreCert.Start();
            }
            return;
        }

        void IgnoreCertSongs()
        {
            try
            {
                ProgressDisableUI("Ignoring selected played songs...");
                List<TblSong> DBSongQueryList = DBCon.TblSong.ToList();
                foreach (object SelItem in lb_ScrobbleListBox.SelectedItems)
                {
                    ScrobbleSongsList SelectedItem = (ScrobbleSongsList)SelItem;
                    TblSong DBSong = DBSongQueryList.Where(x => x.Id == SelectedItem.Id).FirstOrDefault();
                    DBSong.Ignored = 1;
                }
                DBCon.SubmitChanges();

                if (!(bool)vApplicationSettings["LastfmReduceConfirmation"])
                { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Your selected played songs have successfully been ignored from scrobbling and can now be found on the Ignore tab.", "ScrobbleMe", MessageBoxButton.OK); }); }
            }
            catch (Exception ex)
            { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to ignore your selected played songs.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }); }

            Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
            ThreadLoadScrobble.Start();
            return;
        }

        //Unignore Cert Songs Function
        void UnignoreCert_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Check selected songs
            if (lb_IgnoreListBox.SelectedItems.Count == 0)
            {
                if (!(bool)vApplicationSettings["LastfmReduceConfirmation"])
                { MessageBox.Show("There were no songs selected to unignore, please select some songs from the list to unignore first.", "ScrobbleMe", MessageBoxButton.OK); }
                return;
            }

            MessageBoxResult MessageBoxButton_OkCancel = MessageBoxResult.OK;
            if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { MessageBoxButton_OkCancel = MessageBox.Show("Do you want to unignore the songs? You can easily ignore them later on from the scrobble tab.", "ScrobbleMe", MessageBoxButton.OKCancel); }
            if (MessageBoxButton_OkCancel == MessageBoxResult.OK)
            {
                Thread ThreadIgnoreCert = new Thread(() => UnignoreCertSongs());
                ThreadIgnoreCert.Start();
            }
            return;
        }

        void UnignoreCertSongs()
        {
            try
            {
                ProgressDisableUI("Unignoring selected ignored songs...");
                List<TblSong> DBSongQueryList = DBCon.TblSong.ToList();
                foreach (object SelItem in lb_IgnoreListBox.SelectedItems)
                {
                    IgnoredSongsList SelectedItem = (IgnoredSongsList)SelItem;
                    TblSong DBSong = DBSongQueryList.Where(x => x.Id == SelectedItem.Id).FirstOrDefault();
                    DBSong.Ignored = 0;
                }
                DBCon.SubmitChanges();

                if (!(bool)vApplicationSettings["LastfmReduceConfirmation"])
                { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Your selected ignored songs have successfully been unignored, now it's time to scrobble some songs!", "ScrobbleMe", MessageBoxButton.OK); }); }
            }
            catch (Exception ex)
            { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to unignore your selected played songs.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }); }

            Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
            ThreadLoadScrobble.Start();
            return;
        }

        //Unignore All Songs Function
        void UnignoreAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult MessageBoxButton_OkCancel = MessageBoxResult.OK;
            MessageBoxButton_OkCancel = MessageBox.Show("Do you want to unignore all your songs? You can easily ignore them later on again from the scrobble tab.", "ScrobbleMe", MessageBoxButton.OKCancel);
            if (MessageBoxButton_OkCancel == MessageBoxResult.OK)
            {
                Thread ThreadUnignoreAll = new Thread(() => UnignoreAllSongs());
                ThreadUnignoreAll.Start();
            }
            return;
        }

        void UnignoreAllSongs()
        {
            try
            {
                ProgressDisableUI("Unignoring all ignored songs...");
                List<TblSong> DBSongQueryList = DBCon.TblSong.ToList().Where(x => x.Ignored == 1).ToList();
                foreach (TblSong DBSong in DBSongQueryList) { DBSong.Ignored = 0; }
                DBCon.SubmitChanges();

                if (!(bool)vApplicationSettings["LastfmReduceConfirmation"])
                { Dispatcher.BeginInvoke(delegate { MessageBox.Show("All your ignored songs have successfully been unignored, now it's time to scrobble some songs!", "ScrobbleMe", MessageBoxButton.OK); }); }
            }
            catch (Exception ex)
            { Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to unignore all your ignored songs.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }); }

            Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
            ThreadLoadScrobble.Start();
            return;
        }
    }
}