using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using RandomWiki.Model;

namespace RandomWiki
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private bool isDarkTheme;
        private SolidColorBrush accentBrush;
        private SolidColorBrush phoneForegroundBrush;
        private SolidColorBrush phoneBackgroundBrush;

        private RandomWikiModel model;
        public RandomWikiModel Model
        {
            get { return model; }
            set
            {
                if (value != model)
                {
                    model = value;
                    NotifyPropertyChanged("Model");
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();

            isDarkTheme = (Visibility.Visible == (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"]);
            //isLightTheme = (Visibility.Visible == (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"]);
            accentBrush = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];
            phoneForegroundBrush = (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
            phoneBackgroundBrush = (SolidColorBrush)Application.Current.Resources["PhoneBackgroundBrush"];
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var storage = IsolatedStorageSettings.ApplicationSettings;
            Model = State.Load<RandomWikiModel>("Model");

            if (Model == null)
            {
                Model = new RandomWikiModel();

                if (!storage.Contains("setting_language"))
                {
                    Model.Language = new LanguageModel("English", "http://en.wikipedia.org");
                    storage.Add("setting_language", Model.Language);
                    storage.Save();
                }
                else
                {
                    Model.Language = (LanguageModel)storage["setting_language"];
                }

                string url;
                if (NavigationContext.QueryString.TryGetValue("url", out url))
                    LoadWikiArticle(url, true);
                else
                    LoadRandomWikiArticle();
            }
            else
            {
                WikiBrowser.Opacity = 0.0;
            }

            Model.Language = (LanguageModel)storage["setting_language"];
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (Model.WikiUrls.Count > 0)
            {
                e.Cancel = true;

                var last = Model.WikiUrls.Last();
                Model.WikiUrls.Remove(last);
                LoadWikiArticle(last, false);
            }

        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            State.Save("Model", Model);
        }

        private void LoadRandomWikiArticle()
        {
            //LoadWikiArticle(Model.Language.Url + "/wiki/Special:Random", true);
            LoadWikiArticleWithService(Model.Language.Url + "/wiki/Special:Random", true);
            //LoadWikiArticle("http://en.wikipedia.org/wiki/London", true);
        }

        private void LoadWikiArticleWithService(string url, bool storeInStack)
        {
            if (Model.WikiUrl != null && storeInStack)
                Model.WikiUrls.Add(Model.WikiUrl);

            ChangeLoadState(true);

            var wikiService = new WikiService.WikiClient();
            wikiService.GetArticleCompleted += wikiService_GetArticleCompleted;
            wikiService.GetArticleAsync(url);
        }

        void wikiService_GetArticleCompleted(object sender, WikiService.GetArticleCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ChangeLoadState(false);
                    ShowPopupMessage("Connection error");
                });
            }
            else
            {
                var article = e.Result;
                Dispatcher.BeginInvoke(() =>
                {
                    WikiBrowser.Opacity = 0.0;
                    Model.WikiUrl = article.Url;
                    Model.WikiTitle = article.Title;

                    var wikiStyles = "<meta content=\"text/html; charset=UTF-8\" http-equiv=\"Content-Type\"><style>\n";
                    wikiStyles += "a, a:hover, a:visited { text-decoration: none; color:" + ConvertColorToHex(accentBrush.Color) + "; }\n";
                    wikiStyles += "img { border: 0; }\n";
                    wikiStyles += "body { font-family: Segoe UI Light; margin:0; padding:0; background-color:" + ConvertColorToHex(phoneBackgroundBrush.Color) + "; color:" + ConvertColorToHex(phoneForegroundBrush.Color) + ";}\n";
                    wikiStyles += "</style>";

                    Model.WikiHtml = ConvertExtendedASCII(string.Format("<html><head>{1}</head><body bgcolor=\"" + ConvertColorToHex(phoneBackgroundBrush.Color) + "\"><p>{0}</p></body></html>", article.Body, wikiStyles));
                    WikiBrowser.NavigateToString(Model.WikiHtml);
                    //WikiBrowser.NavigateToString(GetWikiHtml(html));
                    //WikiListBox.UpdateLayout();
                    //WikiListBox.ScrollIntoView(WikiListBox.Items.First());
                    ChangeLoadState(false);
                });
            }
        }

        private void LoadWikiArticle(string url, bool storeInStack)
        {
            if (Model.WikiUrl != null && storeInStack)
                Model.WikiUrls.Add(Model.WikiUrl);

            ChangeLoadState(true);

            var request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.BeginGetResponse((result) =>
            {
                WebResponse response = null;

                try
                {
                    response = request.EndGetResponse(result);
                }
                catch
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        ChangeLoadState(false);
                        ShowPopupMessage("Connection error");
                    });
                }

                if (response != null)
                {
                    var sr = new StreamReader(response.GetResponseStream());
                    var html = sr.ReadToEnd();
                    sr.Close();
                    response.Close();

                    html = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(html), 0, html.Length);

                    


                    Dispatcher.BeginInvoke(() =>
                    {
                        WikiBrowser.Opacity = 0.0;
                        Model.WikiUrl = response.ResponseUri.ToString();
                        Model.WikiTitle = GetWikiTitle(html);
                        //WikiText.Text = GetWikiText(html);
                        var wikiStyles = "<meta content=\"text/html; charset=UTF-8\" http-equiv=\"Content-Type\"><style>\n";
                        wikiStyles += "a, a:hover, a:visited { text-decoration: none; color:" + ConvertColorToHex(accentBrush.Color) + "; }\n";
                        wikiStyles += "img { border: 0; }\n";
                        wikiStyles += "body { font-family: Segoe UI Light; margin:0; padding:0; background-color:" + ConvertColorToHex(phoneBackgroundBrush.Color) + "; color:" + ConvertColorToHex(phoneForegroundBrush.Color) + ";}\n";
                        wikiStyles += "</style>";

                        Model.WikiHtml = ConvertExtendedASCII(string.Format("<html><head>{1}</head><body bgcolor=\"" + ConvertColorToHex(phoneBackgroundBrush.Color) + "\"><p>{0}</p></body></html>", GetWikiText(html), wikiStyles));
                        WikiBrowser.NavigateToString(Model.WikiHtml);
                        //WikiBrowser.NavigateToString(GetWikiHtml(html));
                        //WikiListBox.UpdateLayout();
                        //WikiListBox.ScrollIntoView(WikiListBox.Items.First());
                        ChangeLoadState(false);
                    });
                }
            }, request);
        }

        private static string ConvertExtendedASCII(string HTML)
        {
            string retVal = "";
            char[] s = HTML.ToCharArray();

            foreach (char c in s)
            {
                if (Convert.ToInt32(c) > 127)
                    retVal += "&#" + Convert.ToInt32(c) + ";";
                else
                    retVal += c;
            }

            return retVal;
        }  

        private string ConvertColorToHex(Color color)
        {
            return String.Format("#{0:X2}{1:X2}{2:X2}",
                color.R,
                color.G,
                color.B);
        }

        private string GetWikiTitle(string p)
        {
            var match = Regex.Match(p, "<h1((.|\\n)*?)>((.|\\n)*?)<\\/h1>");

            var result = match.Groups[3].Value;
            result = Regex.Replace(result, @"<i>((.|\n)*?)<\/i>", "$1", RegexOptions.Multiline );
            result = Regex.Replace(result, @"<b>((.|\n)*?)<\/b>", "$1", RegexOptions.Multiline );
            return HttpUtility.HtmlDecode(result);
        }

        private string GetWikiText(string e)
        {
            var watch = new Stopwatch();
            watch.Start();

            var result = e.Substring(e.IndexOf("<div id=\"bodyContent\">") + 22);
            result = Regex.Replace(result, @"<!((.|\n)*?)>", "", RegexOptions.Multiline);
            result = Regex.Replace(result, @"<table((.|\n)*?)<\/table>", "", RegexOptions.Multiline);
            result = Regex.Replace(result, @"<tr((.|\n)*?)<\/tr>", "", RegexOptions.Multiline);
            result = Regex.Replace(result, @"<td((.|\n)*?)<\/td>", "", RegexOptions.Multiline);
            
            if (result.Length > 20000)
            {
                result = result.Substring(0, 20000);
                if (result.LastIndexOf("<") > result.LastIndexOf(">"))
                    result = result.Substring(0, result.LastIndexOf("<"));
                else
                    result = result.Substring(0, result.LastIndexOf(".")) + ".";
            }

            result = Regex.Replace(result, "</tr>", "");
            result = Regex.Replace(result, "</td>", "");
            result = Regex.Replace(result, "</table>", "");
            result = Regex.Replace(result, @"<script((.|\n)*?)<\/script>", "", RegexOptions.Multiline);
            result = Regex.Replace(result, @"<div((.|\n)*?)<\/div>", "", RegexOptions.Multiline);
            result = Regex.Replace(result, "<sup((.|\\n)*?)<\\/sup>", "", RegexOptions.Multiline);
            result = Regex.Replace(result, "<a((.|\\n)*?)href=\"/((.|\\n)*?)\"", "<a href=\"" + Model.WikiUrl.Substring(0,Model.WikiUrl.IndexOf("/wiki/")) + "/$3\"", RegexOptions.Multiline);
            result = Regex.Replace(result, "<span class=\"editsection\">((.|\\n)*?)<\\/span>", "", RegexOptions.Multiline);
            result = Regex.Replace(result, @"\t", "");
            result = Regex.Replace(result, @"\n", "");


            //result = Regex.Replace(result, @"<p>((.|\n)*?)<\/p>", "$1", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<a((.|\n)*?)>((.|\n)*?)<\/a>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<ol((.|\n)*?)>((.|\n)*?)<\/ol>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<li((.|\n)*?)>((.|\n)*?)<\/li>", "- $3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<i>((.|\n)*?)<\/i>", "$1", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<b>((.|\n)*?)<\/b>", "$1", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<h1((.|\n)*?)>((.|\n)*?)<\/h1>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<h2((.|\n)*?)>((.|\n)*?)<\/h2>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<h3((.|\n)*?)>((.|\n)*?)<\/h3>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<h4((.|\n)*?)>((.|\n)*?)<\/h4>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<h5((.|\n)*?)>((.|\n)*?)<\/h5>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<h6((.|\n)*?)>((.|\n)*?)<\/h6>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<span((.|\n)*?)>((.|\n)*?)<\/span>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, "<table class=\"infobox vcard\" style=\"font-size: 85%;\">((.|\\n)*?)<\\/table>", "", RegexOptions.Multiline );
            //result = Regex.Replace(result, "<table id=\"toc\" class=\"toc\">((.|\\n)*?)<\\/table>", "", RegexOptions.Multiline );

            //result = Regex.Replace(result, @"<\/((.|\n)*?)>", "", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<((.|\n)*?)>((.|\n)*?)<\/((.|\n)*?)>", "", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<((.|\n)*?)>((.|\n)*?)<\/>", "$3", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"\t", "", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"\n", "", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"\[((.|\n)*?)\]", "", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"<((.|\n)*?)>", "", RegexOptions.Multiline );
            //result = Regex.Replace(result, @"\n ", "\n\n", RegexOptions.Multiline );

            

            Debug.WriteLine(watch.ElapsedMilliseconds);
            
            return result;
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            LoadRandomWikiArticle();
        }

        private void PageTitle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var browserTask = new WebBrowserTask();
            browserTask.URL = Model.WikiUrl;
            browserTask.Show();
        }

        private void ShowPopupMessage(string message)
        {
            PopupMessage.Text = message;
            Storyboard popupStoryboard = (Storyboard) PopupGrid.Resources["PopupStoryBoard"];
            popupStoryboard.Begin();
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            PageTitle_MouseLeftButtonUp(this, null);
        }

        private void ChangeLoadState(bool isLoading)
        {
            LoaderProgressBar.IsIndeterminate = isLoading;
            LoaderProgressBar.Visibility = isLoading ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = !isLoading;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = !isLoading;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = !isLoading;
        }

        private void WikiBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            e.Cancel = true;
            var url = e.Uri.ToString();

            if (url.Contains(".wikipedia.org/wiki/"))
            {
                LoadWikiArticle(url, true);
            }
            else
            {
                var browserTask = new WebBrowserTask();
                browserTask.URL = url;
                browserTask.Show();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void WikiBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            if (Model != null && Model.WikiHtml != null)
                WikiBrowser.NavigateToString(Model.WikiHtml);
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            var storage = IsolatedStorageSettings.ApplicationSettings;
            List<WikiModel> favorites;

            if (!storage.Contains("favorites"))
            {
                favorites = new List<WikiModel>();
                storage.Add("favorites", favorites);
            }
            else
            {
                favorites = (List<WikiModel>)storage["favorites"];
            }

            WikiModel favorite = favorites.SingleOrDefault(f => f.Url == Model.WikiUrl);

            if (favorite != null)
                favorite.Title = Model.WikiTitle;
            else
                favorites.Add(new WikiModel(Model.WikiTitle, Model.WikiUrl));

            storage.Save();


            ShowPopupMessage("Added article to favorites");
        }

        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Favorites.xaml", UriKind.Relative));
        }

        private void WikiBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            WikiBrowser.Opacity = 1.0;
        }
    }
}