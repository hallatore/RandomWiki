using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RandomWiki.Model
{
    public class RandomWikiModel : INotifyPropertyChanged
    {
        public RandomWikiModel()
        {
            WikiUrls = new List<string>();
        }

        public string WikiUrl { get; set; }
        public string WikiHtml { get; set; }

        private String wikiTitle;
        public String WikiTitle
        {
            get { return wikiTitle; }
            set
            {
                if (value != wikiTitle)
                {
                    wikiTitle = value;
                    NotifyPropertyChanged("WikiTitle");
                }
            }
        }

        public List<String> WikiUrls { get; set; }
        public LanguageModel Language { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
