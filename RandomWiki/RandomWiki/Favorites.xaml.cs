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
using Microsoft.Phone.Tasks;
using RandomWiki.Model;

namespace RandomWiki
{
    public partial class Favorites : PhoneApplicationPage
    {
        public Favorites()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadFavorites();
        }

        private void LoadFavorites()
        {
            var storage = IsolatedStorageSettings.ApplicationSettings;
            List<WikiModel> favorites;

            if (!storage.Contains("favorites"))
            {
                favorites = new List<WikiModel>();
            }
            else
            {
                favorites = (List<WikiModel>)storage["favorites"];
            }

            FavoritesListBox.ItemsSource = favorites.OrderBy(f => f.Title);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var storage = IsolatedStorageSettings.ApplicationSettings;
            var favorites = (List<WikiModel>)storage["favorites"];

            var menuItem = (MenuItem)sender;
            var favorite = (WikiModel)menuItem.DataContext;
            favorites.Remove(favorite);

            storage.Save();

            LoadFavorites();
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var menuItem = (StackPanel)sender;
            var favorite = (WikiModel)menuItem.DataContext;

            NavigationService.Navigate(new Uri("/MainPage.xaml?url=" + favorite.Url, UriKind.Relative));
        }
    }
}