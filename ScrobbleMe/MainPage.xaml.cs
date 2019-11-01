using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows.Controls.Primitives;

namespace ScrobbleMe
{
    partial class MainPage : PhoneApplicationPage
    {
        //Application Variables
        StandardTileData vStandardTileData = new StandardTileData();
        IsolatedStorageSettings vApplicationSettings = IsolatedStorageSettings.ApplicationSettings;
        PhoneApplicationService vPhoneApplicationService = PhoneApplicationService.Current;
        ProgressIndicator vProgressIndicator = new ProgressIndicator();
        MediaLibrary vMediaLibrary = new MediaLibrary();
        WebBrowserTask vWebBrowserTask = new WebBrowserTask();

        //Application Startup
        public MainPage() { InitializeComponent(); }

        //Application Navigation
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                ShowLoadSplashScreen();
                LoadScrobbleMe();
            }
            catch { }
        }

        //Load ScrobbleMe Data
        async void LoadScrobbleMe()
        {
            try
            {
                LoadSettings();
                LoadHelp();
                LoadDatabase();

                //Check account name and password
                if (!String.IsNullOrWhiteSpace(vApplicationSettings["LastfmSessionKey"].ToString()) && (bool)vApplicationSettings["LoadLastFMData"])
                {
                    await LoadLoved();
                    await LoadRecent();
                    await LoadArtists();
                    //await LoadLikes();
                }

                Thread ThreadLoadScrobble = new Thread(() => LoadScrobbleIgnore());
                ThreadLoadScrobble.Start();

                //Tab navigation
                if (String.IsNullOrWhiteSpace(vApplicationSettings["LastfmAccount"].ToString()) || String.IsNullOrWhiteSpace(vApplicationSettings["LastfmPassword"].ToString()))
                { MainPivot.SelectedItem = Settings; }
                else
                {
                    switch ((int)vApplicationSettings["StartupTab"])
                    {
                        case 1: { MainPivot.SelectedItem = Ignore; break; }
                        case 2: { MainPivot.SelectedItem = Loved; break; }
                        case 3: { MainPivot.SelectedItem = Recent; break; }
                        case 4: { MainPivot.SelectedItem = Artists; break; }
                        //case 5: { MainPivot.SelectedItem = Like; break; }
                        case 5: { MainPivot.SelectedItem = Settings; break; }
                        case 6: { MainPivot.SelectedItem = Settings; break; }
                    }
                }
            }
            catch { }
            return;
        }

        //Load Splash Screen
        static Popup ShowPopup { get; set; }
        void ShowLoadSplashScreen()
        {
            if (ShowPopup == null)
            {
                ShowPopup = new Popup();
                ShowPopup.Child = new LoadSplashScreen();
                ShowPopup.IsOpen = true;
            }
        }
        void HideLoadSplashScreen() { if (ShowPopup != null) { ShowPopup.IsOpen = false; } }

        //Progressbar/UI Status
        void ProgressDisableUI(string ProgressMsg)
        {
            Dispatcher.BeginInvoke(delegate
            {
                //Enable progressbar
                SystemTray.SetIsVisible(this, true);
                vProgressIndicator.IsVisible = true;
                vProgressIndicator.IsIndeterminate = true;
                vProgressIndicator.Text = ProgressMsg;
                SystemTray.SetProgressIndicator(this, vProgressIndicator);

                //Disable UI buttons
                Scrobble.IsEnabled = false;
                Loved.IsEnabled = false;
                Recent.IsEnabled = false;
                Artists.IsEnabled = false;
                //Like.IsEnabled = false;
                Settings.IsEnabled = false;
                Ignore.IsEnabled = false;
            });

            //Prevent application lock screen
            vPhoneApplicationService.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            return;
        }

        void ProgressEnableUI()
        {
            Dispatcher.BeginInvoke(delegate
            {
                //Disable progressbar
                SystemTray.SetIsVisible(this, true);
                vProgressIndicator.IsVisible = false;
                SystemTray.SetProgressIndicator(this, vProgressIndicator);

                //Enable UI buttons
                Scrobble.IsEnabled = true;
                Loved.IsEnabled = true;
                Recent.IsEnabled = true;
                Artists.IsEnabled = true;
                //Like.IsEnabled = true;
                Settings.IsEnabled = true;
                Ignore.IsEnabled = true;
            });

            //Allow application lock screen
            vPhoneApplicationService.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            return;
        }
    }
}