using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using RandomWiki.Model;

namespace RandomWiki
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var storage = IsolatedStorageSettings.ApplicationSettings;

            var languages = new List<LanguageModel>();
            languages.Add(new LanguageModel("Danish", "http://da.wikipedia.org"));
            languages.Add(new LanguageModel("English", "http://en.wikipedia.org"));
            languages.Add(new LanguageModel("Norwegian", "http://no.wikipedia.org"));
            languages.Add(new LanguageModel("Swedish", "http://sv.wikipedia.org"));

            languageListPicker.ItemsSource = languages;

            if (storage.Contains("setting_language"))
            {
                var language = storage["setting_language"] as LanguageModel;

                if (language != null)
                {
                    var listLanguage = languages.SingleOrDefault(l => l.Name == language.Name);
                    if (listLanguage != null)
                        languageListPicker.SelectedItem = listLanguage;
                }
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            var storage = IsolatedStorageSettings.ApplicationSettings;

            if (storage.Contains("setting_language"))
            {
                storage["setting_language"] = languageListPicker.SelectedItem;
            }
            else
            {
                storage.Add("setting_language", languageListPicker.SelectedItem);
            }

            storage.Save();
        }
    }
}