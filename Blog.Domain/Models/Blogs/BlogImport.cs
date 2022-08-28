using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Domain.Models.Blogs
{
    public class BlogImport
    {
        public string title { get; set; }
        public string description { get; set; }
        public string publication_date { get; set; }

        //All imported blog by Admin is 0
        public int UserId { get; set; } = 0;
    }


    public class BlogImportResponse
    {
        public List<BlogImport> data { get; set; }
    }
}
