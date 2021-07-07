using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Models
{
    public class Ban
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        [MaxLength(40)]
        public string IpAddress { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public DateTime? EndDate { get; set; }

        public bool CurrentlyBanned => EndDate == null || EndDate.Value > DateTime.Now;
    }
}
