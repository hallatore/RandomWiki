using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace WikimizeService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class Service : IService
    {
        public Article GetArticle(string url)
        {
            var request = HttpWebRequest.Create(url) as HttpWebRequest;
            var response = request.GetResponse();

            var sr = new StreamReader(response.GetResponseStream());
            var html = sr.ReadToEnd();
            sr.Close();
            response.Close();

            html = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(html), 0, html.Length);

            return new Article
            {
                Url = response.ResponseUri.ToString(),
                Title = GetWikiTitle(html),
                Body = GetWikiBody(html, response.ResponseUri.ToString())
            };
        }



        private string GetWikiTitle(string p)
        {
            var match = Regex.Match(p, "<h1((.|\\n)*?)>((.|\\n)*?)<\\/h1>");

            var result = match.Groups[3].Value;
            result = Regex.Replace(result, @"<i>((.|\n)*?)<\/i>", "$1", RegexOptions.Multiline);
            result = Regex.Replace(result, @"<b>((.|\n)*?)<\/b>", "$1", RegexOptions.Multiline);
            return HttpUtility.HtmlDecode(result);
        }

        private string GetWikiBody(string e, string url)
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
            result = Regex.Replace(result, "<a((.|\\n)*?)href=\"/((.|\\n)*?)\"", "<a href=\"" + url.Substring(0, url.IndexOf("/wiki/")) + "/$3\"", RegexOptions.Multiline);
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
    }
}
