using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.ViewModels
{
    public class MinPostViewModel
    {
        public long ID { get; set; }
        public string Title { get; set; }
        public string Stub { get; set; }
        public DateTime PublishDate { get; set; }
    }
}
