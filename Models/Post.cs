using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dev_Blog.Models
{
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        [MaxLength(80)]
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DateCreated { get; set; }
        public DateTime? DatePublished { get; set; }
        public DateTime? DateModified { get; set; }
        public DateTime? DateDeleted { get; set; }


        [Required]
        public long AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User Author { get; set; }

        public List<Comment> Comments { get; set; }

        [NotMapped]
        public bool Deleted => DateDeleted != null;
    }
}
