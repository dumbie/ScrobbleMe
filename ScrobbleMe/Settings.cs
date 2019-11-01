using Md5Encryption;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScrobbleMe
{
    partial class MainPage
    {
        //Settings UI Functions
        void txt_SettingsLastfmAccount_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                if (String.IsNullOrEmpty(txt_SettingsLastfmPassword.Password)) { txt_SettingsLastfmPassword.Focus(); }
                else { btn_SettingsLoginLastFM_Click(null, null); }
            }
        }

        void txt_SettingsLastfmPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                if (String.IsNullOrEmpty(txt_SettingsLastfmAccount.Text)) { txt_SettingsLastfmAccount.Focus(); }
                else { btn_SettingsLoginLastFM_Click(null, null); }
            }
        }

        //Load - Application Settings
        void LoadSettings()
        {
            try
            {
                //Set application styles
                (Resources["PhoneAccentBrush"] as SolidColorBrush).Color = Color.FromArgb(255, 220, 19, 3);
                lp_SettingStartupTab.SetValue(ListPicker.ItemCountThresholdProperty, 10);

                //Load - Lastest scrobble time
                if (!vApplicationSettings.Contains("LastfmScrobbled"))
                { vApplicationSettings["LastfmScrobbled"] = "you have never scrobbled."; }

                //Load - Last.fm session key
                if (!vApplicationSettings.Contains("LastfmSessionKey"))
                { vApplicationSettings["LastfmSessionKey"] = ""; }

                //Load - Account and password
                if (!vApplicationSettings.Contains("LastfmAccount"))
                { vApplicationSettings["LastfmAccount"] = ""; }
                else { txt_SettingsLastfmAccount.Text = vApplicationSettings["LastfmAccount"].ToString(); }
                if (!vApplicationSettings.Contains("LastfmPassword"))
                { vApplicationSettings["LastfmPassword"] = ""; }
                else { txt_SettingsLastfmPassword.Password = vApplicationSettings["LastfmPassword"].ToString(); }

                //Load - Last.fm Delay Time
                if (!vApplicationSettings.Contains("LastfmDelayTimeInt"))
                {
                    vApplicationSettings["LastfmDelayTimeInt"] = 10;
                    txt_SettingsLastfmDelayTime.Text = "10";
                }
                else { txt_SettingsLastfmDelayTime.Text = vApplicationSettings["LastfmDelayTimeInt"].ToString(); }

                //Load - Scrobble Artist Order
                if (!vApplicationSettings.Contains("LastfmScrobbleArtistOrder"))
                {
                    vApplicationSettings["LastfmScrobbleArtistOrder"] = false;
                    cb_SettingsScrobbleArtistOrder.IsChecked = false;
                }
                else { cb_SettingsScrobbleArtistOrder.IsChecked = (bool)vApplicationSettings["LastfmScrobbleArtistOrder"]; }

                //Load - Scrobble Song Order
                if (!vApplicationSettings.Contains("LastfmScrobbleSongOrder"))
                {
                    vApplicationSettings["LastfmScrobbleSongOrder"] = false;
                    cb_SettingsScrobbleSongOrder.IsChecked = false;
                }
                else { cb_SettingsScrobbleSongOrder.IsChecked = (bool)vApplicationSettings["LastfmScrobbleSongOrder"]; }

                //Load - Application Startup tab
                if (!vApplicationSettings.Contains("StartupTab"))
                {
                    vApplicationSettings["StartupTab"] = 0;
                    lp_SettingStartupTab.SelectedIndex = 0;
                }
                else { lp_SettingStartupTab.SelectedIndex = Convert.ToInt32(vApplicationSettings["StartupTab"]); }

                //Load - Top artists load period
                if (!vApplicationSettings.Contains("LastfmArtistLoadPeriodInt"))
                {
                    vApplicationSettings["LastfmArtistLoadPeriodInt"] = 4;
                    lp_SettingsArtistLoadPeriod.SelectedIndex = 4;
                }
                else { lp_SettingsArtistLoadPeriod.SelectedIndex = Convert.ToInt32(vApplicationSettings["LastfmArtistLoadPeriodInt"]); }

                //Load - Skip multiple plays
                if (!vApplicationSettings.Contains("SkipMultiplePlays"))
                {
                    vApplicationSettings["SkipMultiplePlays"] = false;
                    cb_SettingsSkipMultiplePlays.IsChecked = false;
                }
                else { cb_SettingsSkipMultiplePlays.IsChecked = (bool)vApplicationSettings["SkipMultiplePlays"]; }

                //Load - Load Last.fm user data
                if (!vApplicationSettings.Contains("LoadLastFMData"))
                {
                    vApplicationSettings["LoadLastFMData"] = true;
                    cb_SettingsLoadLastFMData.IsChecked = true;
                }
                else { cb_SettingsLoadLastFMData.IsChecked = (bool)vApplicationSettings["LoadLastFMData"]; }

                //Load - Download Covers
                if (!vApplicationSettings.Contains("LastfmDownloadCovers"))
                {
                    vApplicationSettings["LastfmDownloadCovers"] = true;
                    cb_SettingsDownloadCovers.IsChecked = true;
                }
                else { cb_SettingsDownloadCovers.IsChecked = (bool)vApplicationSettings["LastfmDownloadCovers"]; }

                //Load - Reduce Confirmation
                if (!vApplicationSettings.Contains("LastfmReduceConfirmation"))
                {
                    vApplicationSettings["LastfmReduceConfirmation"] = false;
                    cb_SettingsReduceConfirmation.IsChecked = false;
                }
                else { cb_SettingsReduceConfirmation.IsChecked = (bool)vApplicationSettings["LastfmReduceConfirmation"]; }

                //Load - Show scrobble tile count
                if (!vApplicationSettings.Contains("ScrobbleTileCount"))
                {
                    vApplicationSettings["ScrobbleTileCount"] = true;
                    cb_SettingsScrobbleTileCount.IsChecked = true;
                }
                else { cb_SettingsScrobbleTileCount.IsChecked = (bool)vApplicationSettings["ScrobbleTileCount"]; }

                vApplicationSettings.Save();
                SettingsSaveEvents();
            }
            catch (Exception ex)
            { MessageBox.Show("Failed to load your settings.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }
            return;
        }

        //Save Events - Application Settings
        void SettingsSaveEvents()
        {
            try
            {
                //Save - Last.fm Delay Time
                txt_SettingsLastfmDelayTime.TextChanged += (sender, e) =>
                {
                    if (!String.IsNullOrWhiteSpace(txt_SettingsLastfmDelayTime.Text))
                    {
                        if (Regex.IsMatch(txt_SettingsLastfmDelayTime.Text, "(\\D+)"))
                        {
                            MessageBox.Show("The Last.fm delay time can only contain numbers, please check your delay time.", "ScrobbleMe", MessageBoxButton.OK);
                            int Selection = txt_SettingsLastfmDelayTime.SelectionStart;
                            txt_SettingsLastfmDelayTime.Text = txt_SettingsLastfmDelayTime.Text.Remove(Selection - 1, 1);
                            txt_SettingsLastfmDelayTime.Select(Selection, 0);
                            return;
                        }

                        if (Convert.ToInt32(txt_SettingsLastfmDelayTime.Text) > 540)
                        {
                            MessageBox.Show("Please enter a valid Last.fm delay time, 540 minutes (9 hours) is the maximum.", "ScrobbleMe", MessageBoxButton.OK);
                            int Selection = txt_SettingsLastfmDelayTime.SelectionStart;
                            txt_SettingsLastfmDelayTime.Text = txt_SettingsLastfmDelayTime.Text.Remove(Selection - 1, 1);
                            txt_SettingsLastfmDelayTime.Select(Selection, 0);
                            return;
                        }

                        if (txt_SettingsLastfmDelayTime.Text.Length > 1 && txt_SettingsLastfmDelayTime.Text.StartsWith("0"))
                        {
                            MessageBox.Show("Please enter a valid Last.fm delay time, 540 minutes (9 hours) is the maximum.", "ScrobbleMe", MessageBoxButton.OK);
                            int Selection = txt_SettingsLastfmDelayTime.SelectionStart;
                            txt_SettingsLastfmDelayTime.Text = txt_SettingsLastfmDelayTime.Text.Remove(Selection - 1, 1);
                            txt_SettingsLastfmDelayTime.Select(Selection, 0);
                            return;
                        }

                        vApplicationSettings["LastfmDelayTimeInt"] = Convert.ToInt32(txt_SettingsLastfmDelayTime.Text);
                    }
                };

                //Save - Scrobble Artist Order
                cb_SettingsScrobbleArtistOrder.Click += (sender, e) =>
                {
                    CheckBox CheckBox = sender as CheckBox;
                    if ((bool)CheckBox.IsChecked)
                    { vApplicationSettings["LastfmScrobbleArtistOrder"] = true; }
                    else { vApplicationSettings["LastfmScrobbleArtistOrder"] = false; }
                    vApplicationSettings.Save();
                };

                //Save - Scrobble Song Order
                cb_SettingsScrobbleSongOrder.Click += (sender, e) =>
                {
                    CheckBox CheckBox = sender as CheckBox;
                    if ((bool)CheckBox.IsChecked)
                    { vApplicationSettings["LastfmScrobbleSongOrder"] = true; }
                    else { vApplicationSettings["LastfmScrobbleSongOrder"] = false; }
                    vApplicationSettings.Save();
                };

                //Save - Application Startup Tab
                lp_SettingStartupTab.SelectionChanged += (sender, e) =>
                {
                    ListPicker ListPicker = sender as ListPicker;
                    if ((int)vApplicationSettings["StartupTab"] != ListPicker.SelectedIndex)
                    {
                        vApplicationSettings["StartupTab"] = ListPicker.SelectedIndex;
                        vApplicationSettings.Save();
                    }
                };

                //Save - Top artists load period
                lp_SettingsArtistLoadPeriod.SelectionChanged += (sender, e) =>
                {
                    ListPicker ListPicker = sender as ListPicker;
                    if ((int)vApplicationSettings["LastfmArtistLoadPeriodInt"] != ListPicker.SelectedIndex)
                    {
                        vApplicationSettings["LastfmArtistLoadPeriodInt"] = ListPicker.SelectedIndex;
                        vApplicationSettings.Save();
                    }
                };

                //Save - Skip multiple plays
                cb_SettingsSkipMultiplePlays.Click += (sender, e) =>
                {
                    CheckBox CheckBox = sender as CheckBox;
                    if ((bool)CheckBox.IsChecked)
                    { vApplicationSettings["SkipMultiplePlays"] = true; }
                    else { vApplicationSettings["SkipMultiplePlays"] = false; }
                    vApplicationSettings.Save();
                };

                //Save - Load Last.fm user data
                cb_SettingsLoadLastFMData.Click += (sender, e) =>
                {
                    CheckBox CheckBox = sender as CheckBox;
                    if ((bool)CheckBox.IsChecked)
                    { vApplicationSettings["LoadLastFMData"] = true; }
                    else { vApplicationSettings["LoadLastFMData"] = false; }
                    vApplicationSettings.Save();
                };

                //Save - Download covers
                cb_SettingsDownloadCovers.Click += (sender, e) =>
                {
                    CheckBox CheckBox = sender as CheckBox;
                    if ((bool)CheckBox.IsChecked)
                    { vApplicationSettings["LastfmDownloadCovers"] = true; }
                    else { vApplicationSettings["LastfmDownloadCovers"] = false; }
                    vApplicationSettings.Save();
                };

                //Save - Reduce confirmation
                cb_SettingsReduceConfirmation.Click += (sender, e) =>
                {
                    CheckBox CheckBox = sender as CheckBox;
                    if ((bool)CheckBox.IsChecked)
                    { vApplicationSettings["LastfmReduceConfirmation"] = true; }
                    else { vApplicationSettings["LastfmReduceConfirmation"] = false; }
                    vApplicationSettings.Save();
                };

                //Save - Show scrobble tile count
                cb_SettingsScrobbleTileCount.Click += (sender, e) =>
                {
                    CheckBox CheckBox = sender as CheckBox;
                    if ((bool)CheckBox.IsChecked)
                    {
                        vApplicationSettings["ScrobbleTileCount"] = true;
                        Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
                        ThreadLoadScrobble.Start();
                    }
                    else
                    {
                        vApplicationSettings["ScrobbleTileCount"] = false;
                        ShellTile ShellTile = ShellTile.ActiveTiles.FirstOrDefault();
                        if (ShellTile != null)
                        {
                            vStandardTileData.Title = "";
                            ShellTile.Update(vStandardTileData);
                        }
                    }
                    vApplicationSettings.Save();
                };
            }
            catch (Exception ex)
            { MessageBox.Show("Failed to save your settings.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); }
            return;
        }

        async void btn_SettingsLoginLastFM_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                //Check account name and password
                if (!String.IsNullOrWhiteSpace(txt_SettingsLastfmAccount.Text) && !String.IsNullOrWhiteSpace(txt_SettingsLastfmPassword.Password))
                {
                    vApplicationSettings["LastfmAccount"] = txt_SettingsLastfmAccount.Text.ToLower();
                    if (vApplicationSettings["LastfmPassword"].ToString() != txt_SettingsLastfmPassword.Password)
                    {
                        vApplicationSettings["LastfmPassword"] = MD5CryptoServiceProvider.GetMd5String(txt_SettingsLastfmPassword.Password);
                        txt_SettingsLastfmPassword.Password = vApplicationSettings["LastfmPassword"].ToString();
                    }
                    vApplicationSettings.Save();

                    await LastFMAuth();
                    if ((bool)vApplicationSettings["LoadLastFMData"])
                    {
                        await LoadLoved();
                        await LoadRecent();
                        await LoadArtists();
                        //await LoadLikes();
                    }
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(txt_SettingsLastfmAccount.Text)) { txt_SettingsLastfmAccount.Focus(); }
                    else if (String.IsNullOrWhiteSpace(txt_SettingsLastfmPassword.Password)) { txt_SettingsLastfmPassword.Focus(); }
                    MessageBox.Show("Please enter your Last.fm account name and password to login to your Last.fm profile.", "ScrobbleMe", MessageBoxButton.OK);
                }
            }
            catch { }
            return;
        }

        void OpenProfile_Click(object sender, RoutedEventArgs e)
        {
            //Check account name and password
            if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()))
            { MessageBox.Show("Please set your Last.fm account name and password to visit your Last.fm profile in the browser.", "ScrobbleMe", MessageBoxButton.OK); }
            else
            {
                vWebBrowserTask.Uri = new Uri("http://m.last.fm/user/" + vApplicationSettings["LastfmAccount"]);
                vWebBrowserTask.Show();
            }
            return;
        }

        void VisitWebsite_Click(object sender, RoutedEventArgs e)
        {
            vWebBrowserTask.Uri = new Uri("http://m.arnoldvink.com/?p=projects");
            vWebBrowserTask.Show();
            return;
        }

        void OpenDonation_Click(object sender, RoutedEventArgs e)
        {
            vWebBrowserTask.Uri = new Uri("http://m.arnoldvink.com/?p=donation");
            vWebBrowserTask.Show();
            return;
        }

        void OpenPrivacy_Click(object sender, RoutedEventArgs e)
        {
            vWebBrowserTask.Uri = new Uri("http://privacy.arnoldvink.com");
            vWebBrowserTask.Show();
            return;
        }
    }
}