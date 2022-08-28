using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Domain.Models.Response
{
    public class ApiResponse<T>
    {
        public T Payload { get; set; }
        public bool IsSucessFull { get; set; }
        public string Message { get; set; }

    }

}
