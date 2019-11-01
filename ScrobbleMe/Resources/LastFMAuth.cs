using Md5Encryption;
using System;
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
        async Task LastFMAuth()
        {
            try
            {
                //Check if there is an internet connection
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    MessageBox.Show("It seems like you currently don't have an internet connection available, please make sure you have an internet connection before you try to login.", "ScrobbleMe", MessageBoxButton.OK);
                    return;
                }
                else
                {
                    //Auth to Last.fm
                    ProgressDisableUI("Logging in to your Last.fm profile...");
                    string LastFMApiKey = "a62159e276986acf81f6990148b06ae3"; //Yes, I know I didn't remove the api key.
                    string LastFMApiSecret = "fa570ce8eeb81a3e1685b0e8a27d6517"; //Yes, I know I didn't remove the api key.
                    string LastFMMethod = "auth.getMobileSession";
                    string LastFMAuthToken = MD5CryptoServiceProvider.GetMd5String(vApplicationSettings["LastfmAccount"].ToString().ToLower() + vApplicationSettings["LastfmPassword"].ToString());
                    string LastFMApiSig = MD5CryptoServiceProvider.GetMd5String("api_key" + LastFMApiKey + "authToken" + LastFMAuthToken + "method" + LastFMMethod + "username" + vApplicationSettings["LastfmAccount"].ToString().ToLower() + LastFMApiSecret);

                    XDocument ResponseXml = null;
                    using (HttpClient HttpClientLogin = new HttpClient())
                    {
                        HttpClientLogin.DefaultRequestHeaders.Add("User-Agent", "ScrobbleMe");
                        HttpClientLogin.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
                        HttpClientLogin.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store");

                        Uri PostUri = new Uri("https://ws.audioscrobbler.com/2.0/");
                        HttpStringContent PostContent = new HttpStringContent("api_key=" + HttpUtility.UrlEncode(LastFMApiKey) + "&authToken=" + HttpUtility.UrlEncode(LastFMAuthToken) + "&method=" + HttpUtility.UrlEncode(LastFMMethod) + "&username=" + HttpUtility.UrlEncode(vApplicationSettings["LastfmAccount"].ToString().ToLower()) + "&api_sig=" + HttpUtility.UrlEncode(LastFMApiSig), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");

                        ResponseXml = XDocument.Parse((await HttpClientLogin.PostAsync(PostUri, PostContent)).Content.ToString());
                    }

                    if (ResponseXml.Element("lfm").Attribute("status").Value == "ok")
                    {
                        if (!(bool)vApplicationSettings["LastfmReduceConfirmation"]) { Dispatcher.BeginInvoke(delegate { MessageBox.Show("You have successfully logged into your Last.fm profile, you can now begin to scrobble on the Scrobble tab.", "ScrobbleMe", MessageBoxButton.OK); }); }
                        vApplicationSettings["LastfmSessionKey"] = ResponseXml.Element("lfm").Element("session").Element("key").Value;
                        vApplicationSettings.Save();
                        ProgressEnableUI();
                        return;
                    }
                    else if (!String.IsNullOrEmpty(ResponseXml.Element("lfm").Element("error").Value))
                    {
                        Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to auth with Last.fm, please check your Last.fm account settings or check your internet connection.\n\nError Message: " + ResponseXml.Element("lfm").Element("error").Value, "ScrobbleMe", MessageBoxButton.OK); });
                        vApplicationSettings["LastfmSessionKey"] = "";
                        vApplicationSettings.Save();
                        ProgressEnableUI();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(delegate { MessageBox.Show("Failed to auth with Last.fm, please check your Last.fm account settings or check your internet connection.\n\nException Message: " + ex.Message, "ScrobbleMe", MessageBoxButton.OK); });
                vApplicationSettings["LastfmSessionKey"] = "";
                vApplicationSettings.Save();
                ProgressEnableUI();
                return;
            }
        }
    }
}