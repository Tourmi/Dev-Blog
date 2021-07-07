using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Models
{
    public class PostTag
    {
        public long PostID { get; set; }
        [ForeignKey("PostID")]
        public Post Post { get; set; }
        public string TagID { get; set; }
        [ForeignKey("TagID")]
        public Tag Tag { get; set; }
    }
}
