using AspNetCoreHero.ToastNotification.Abstractions;
using Blog.Domain.Models.Blogs;
using Blog.Domain.Models.Response;
using Blog.Web.Models;
using Blog.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Blog.Web.Controllers
{
    public class BlogsController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly IHttpServices _httpServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSettings _appSettings;

        public BlogsController(INotyfService notyf, IOptions<AppSettings> appSettings,IHttpServices httpServices, IHttpContextAccessor httpContextAccessor)
        {
            _notyf = notyf;
            _httpServices = httpServices;
            _appSettings = appSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Index(string orderby = "dsc")
        {
            string url = $"{_appSettings.ApiUrl}/Blogs/GetAllBlogs";
            var dataResponse = _httpServices.Get(url);
            var blogsResult = JsonConvert.DeserializeObject<ApiResponse<List<BlogResponse>>>(dataResponse.Data);
            if (dataResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
          //      _notyf.Success(blogsResult.Message);
                var list = orderby == "asc" ? blogsResult.Payload.OrderBy(x=>x.PublicationDate).ToList() : blogsResult.Payload.OrderByDescending(x=>x.PublicationDate).ToList();
                return View(list);
            }
            _notyf.Error(blogsResult.Message);
            return View(new List<BlogResponse>());
        }


        [HttpGet]
        public IActionResult UserBlogs()
        {
            string url = $"{_appSettings.ApiUrl}/Blogs/GetUserBlogs";
            var dataResponse = _httpServices.Get(url);
            var blogsResult = JsonConvert.DeserializeObject<ApiResponse<List<BlogResponse>>>(dataResponse.Data);
            if (dataResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
      //          _notyf.Success(blogsResult.Message);
                return View("Index", blogsResult.Payload);
            }
            _notyf.Error(blogsResult.Message);
            return RedirectToAction("Index", "Blogs");
        }

        [HttpGet]
        public IActionResult AddBlog()
        {
            return View(new CreateBlog());
        }


        [HttpPost]
        public IActionResult AddBlog(CreateBlog createBlog)
        {
            if (!ModelState.IsValid)
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _notyf.Error(message);
                return View(createBlog);
            }
            createBlog.PublicationDate = DateTime.Now;
            string url = $"{_appSettings.ApiUrl}/Blogs/CreateBlog";
            var payload = JsonConvert.SerializeObject(createBlog);
            var dataResponse = _httpServices.Post(url, payload);
            var addBlogResult = JsonConvert.DeserializeObject<ApiResponse<int>>(dataResponse.Data);
            if (dataResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _notyf.Success(addBlogResult.Message);
                return RedirectToAction("UserBlogs", "Blogs");
            }
            _notyf.Error(addBlogResult.Message);
            return View(createBlog);
        }


        [HttpGet]
        public IActionResult ImportBlogs()
        {

            string url = $"{_appSettings.ApiUrl}/Blogs/ImportBlogs";
            var dataResponse = _httpServices.Get(url);
            var importResult = JsonConvert.DeserializeObject<ApiResponse<int>>(dataResponse.Data);
            if (dataResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _notyf.Success(importResult.Message);
                return RedirectToAction("Index", "Blogs");
            }
            _notyf.Error(importResult.Message);
            return RedirectToAction("Index", "Blogs");
        }
    }
}
