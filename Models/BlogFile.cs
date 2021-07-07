using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Models
{
    public class BlogFile
    {
        public enum Type
        {
            None, 
            Image,
            Sound
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        [Required]
        [MaxLength(250)]
        public string Path { get; set; }
        public string HtmlType { get; set; }

        [Required]
        public long PostID { get; set; }
        [ForeignKey("PostID")]
        public Post Post { get; set; }
        [NotMapped]
        public Type FileType => HtmlType.Contains("audio") ? Type.Sound : HtmlType.Contains("image") ? Type.Image : Type.None;

    }
}
