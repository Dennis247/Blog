using Blog.api.Services;
using Blog.Domain.Enums;
using Blog.Domain.Models.Blogs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace Blog.api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController : BaseController
    {
        private readonly IBlogService _blogServices;
        public BlogsController(IBlogService blogService)
        {
            _blogServices = blogService;
        }

        [HttpPost("CreateBlog")]
        public IActionResult CreateBlog([FromBody]CreateBlog createBlog)
        {
            var response = _blogServices.CreateBlogPost(Account.Id,createBlog);
            return Ok(response);
        }


        [HttpGet("GetUserBlogs")]
        public IActionResult GetUserBlogs()
        {
            var response = _blogServices.GetBlogsForUser(Account.Id);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("GetAllBlogs")]
        public IActionResult GetAllBlogs()
        {
            var response = _blogServices.GetBlogs();
            return Ok(response);
        }



        [Authorize(Role.Admin)]
        [HttpGet("ImportBlogs")]
        public IActionResult ImportBlogs()
        {
            var response = _blogServices.ImportBlogs();
            return Ok(response);
        }
    }

    


}
