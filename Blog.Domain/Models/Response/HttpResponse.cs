using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Domain.Models.Response
{
    public class WebHttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } 
        public string Data { get; set; } 
    }
}
