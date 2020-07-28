using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Models
{
    public class Subscriber
    {
        public Subscriber()
        {
            SubscribedTo = new HashSet<SubscriberTag>();
        }

        [Key]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string ValidationToken { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool ValidatedEmail { get; set; }
        [Required]
        [DefaultValue(true)]
        public bool SubscribedToAll { get; set; }

        public virtual ICollection<SubscriberTag> SubscribedTo { get; set; }

        public bool IsSubscribedToTag(Tag tag)
        {
            return SubscribedToAll || SubscribedTo.Where(st => st.TagID == tag.Name).Any();
        }
    }
}
