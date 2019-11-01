using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ScrobbleMe
{
    partial class MainPage
    {
        //Help Pivot
        void LoadHelp()
        {
            try
            {
                if (!sp_Help.Children.Any())
                {
                    sp_Help.Children.Add(new TextBlock() { Text = "How do I start scrobbling songs?", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "To be able to scrobble to Last.fm set your account name and password and login on the settings tab then click on 'Scrobble songs' on the Scrobble tab to start scrobbling to Last.fm.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nHelp, it keeps failing to login or scrobble", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "Make sure your Last.fm password is set in ABC and 123 text characters when needed and also make sure your device's time is set correctly.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nWhat does |T|P/S| mean in the scrobble list?", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "The P stands for how many times the song has been played on your device, the S stands for how many times the song has been scrobbled to your Last.fm profile and the T is the track number.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nHelp, I can't see all my played songs!", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "The recently played songs list is limited to 200 songs to improve the application performance, all played songs will get scrobbled so don't worry!", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nHow can I fix unknown played songs?", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "You can fix unknown songs by adding song information to your song files with an application called Mp3tag on your PC which will download covers and song information for you.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nHow can I scrobble streamed songs?", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "Your device only saves the playcount from locally stored songs, songs streamed will not count and unfortunately because of this can't be scrobbled to your Last.fm profile.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nHow can I stop some songs from scrobbling?", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "You can ignore songs from scrobbling to Last.fm by selecting them on the scrobble tab and press on 'Ignore' you can easily unignore the songs from the 'Ignore' tab so they will scrobble again.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nHow is the scrobble date and time decided?", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "The original play time from a song does not get saved on your device that's why ScrobbleMe has the option to delay your scrobbles to Last.fm and will calculate the time based on the songs duration from there on out.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nWhat happens to my scrobbles when I uninstall\nthe ScrobbleMe application from my device?", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "When you uninstall ScrobbleMe all your plays will be reset in the app on reinstallation so it's better to keep the application installed at all times, scrobbles on your Last.fm profile are unaffected.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nHelp, I'm getting 'Root element is missing' error", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "This error means it couldn't connect to the Last.fm server, make sure you have a working internet connection and https capable proxy when you are using a proxy.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nHelp, I'm getting a 'There was an internal' error", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "This could mean the Last.fm servers are in maintenance and won't accept any scrobbles at the moment, please try to scrobble again later.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nHelp, I get 'Songs might have been ignored'", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "This error could mean that you have reached your daily scrobble limit or that a song has invalid information.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nSupport and bug reporting", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "When you are walking into any problems or bugs you can goto the support forum on: http://forum.arnoldvink.com so I can try to help you out and get everything working.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nDevelopment donation support", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "Feel free to make a donation to support me with my developing projects, you can find a donation page on the project website or click below on the donation button to open the donation page.", Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });

                    sp_Help.Children.Add(new TextBlock() { Text = "\r\nApplication made by Arnold Vink", Style = (Style)App.Current.Resources["TextBlock"] });
                    sp_Help.Children.Add(new TextBlock() { Text = "Version: v" + System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0], Style = (Style)App.Current.Resources["TextBlockGrey"], TextWrapping = TextWrapping.Wrap });
                }
            }
            catch { }
        }
    }
}