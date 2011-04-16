using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WikimizeService
{
    [DataContract]
    public class Article
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Body { get; set; }
    }
}