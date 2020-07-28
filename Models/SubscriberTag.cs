using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Models
{
    public class SubscriberTag
    {

        public string SubscriberID { get; set; }
        [ForeignKey("SubscriberID")]
        public Subscriber Subscriber { get; set; }
        public string TagID { get; set; }
        [ForeignKey("TagID")]
        public Tag Tag { get; set; }
    }
}
