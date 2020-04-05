﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Models
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }
        [MaxLength(40)]
        public string IpAddress { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(25)]
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Required]
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public DateTime? DateDeleted { get; set; }

        public long? UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        public long PostID { get; set; }
        [ForeignKey("PostID")]
        public Post Post { get; set; }

        public long? ParentCommentID { get; set; }
        [ForeignKey("ParentCommentID")]
        public Comment ParentComment { get; set; }

        public List<Comment> Comments { get; set; }
    }
}
