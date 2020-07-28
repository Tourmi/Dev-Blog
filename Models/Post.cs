using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Dev_Blog.Models
{
    public class Post
    {
        public Post()
        {
            Comments = new HashSet<Comment>();
            Tags = new HashSet<PostTag>();
            Files = new HashSet<BlogFile>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        [StringLength(80, MinimumLength = 1)]
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        [StringLength(80, MinimumLength = 1)]
        [RegularExpression(@"^[-a-z0-9]+$", ErrorMessage = "The stub should only contain alphanumerics and dashes")]
        public string Stub { get; set; }
        [DisplayName("Thumbnail")]
        [MaxLength(32767)]
        public string ThumbnailPath { get; set; }
        public ulong Views { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DateCreated { get; set; }
        [DisplayName("Published on")]
        public DateTime? DatePublished { get; set; }

        [DisplayName("Last modified on")]
        public DateTime? DateModified { get; set; }
        public DateTime? DateDeleted { get; set; }


        [Required]
        public string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User Author { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<PostTag> Tags { get; set; }
        public virtual ICollection<BlogFile> Files { get; set; }

        [NotMapped]
        public bool Deleted => DateDeleted != null;
        
        [NotMapped]
        public bool Published => DatePublished != null && DatePublished <= DateTime.Now;
    }
}
