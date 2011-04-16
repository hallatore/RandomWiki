using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RandomWiki
{
    public static class StateExtensions
    {
        public static T Load<T>(this IDictionary<string, object> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
                return (T)dictionary[key];

            return default(T);
        }

        public static void Save(this IDictionary<string, object> dictionary, string key, object value)
        {
            if (dictionary.ContainsKey(key))
                dictionary.Remove(key);

            dictionary.Add(key, value);
        }
    }
}
