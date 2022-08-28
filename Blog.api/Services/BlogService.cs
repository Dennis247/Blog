using AutoMapper;
using Blog.api.Helpers;
using Blog.Domain.Context;
using Blog.Domain.Entities;
using Blog.Domain.Models.Blogs;
using Blog.Domain.Models.Response;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace Blog.api.Services
{
    public interface IBlogService
    {
        ApiResponse<int> CreateBlogPost(int UserId,CreateBlog createBlog);
        ApiResponse<List<BlogResponse>> GetBlogsForUser(int UserId);
        ApiResponse<List<BlogResponse>> GetBlogs();
        ApiResponse<int> ImportBlogs();
    }

    public class BlogService : IBlogService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        public BlogService(
         DataContext context,
        IMapper mapper,
        IOptions<AppSettings> appSettings)
        {
            _context = context;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }


        public ApiResponse<int> CreateBlogPost(int UserId,CreateBlog createBlog)
        {
            var blogToCreate = _mapper.Map<BlogPost>(createBlog);
            blogToCreate.UserId = UserId;   
            _context.BlogPosts.Add(blogToCreate);
            _context.SaveChanges();

            return new ApiResponse<int>
            {
                IsSucessFull = true,
                Message = "Blog Post created Sucessful",
                Payload = blogToCreate.Id
            };
        }


        public ApiResponse<List<BlogResponse>> GetBlogsForUser(int UserId)
        {
            getAccount(UserId);
            var userBlogs = _context.BlogPosts.Where(b => b.UserId == UserId).ToList();
            var userBlogsToReturn = _mapper.Map<List<BlogResponse>>(userBlogs);

            return new ApiResponse<List<BlogResponse>>
            {
                IsSucessFull = true,
                Message = "User Blogs Fetched Sucessful",
                Payload = userBlogsToReturn
            };
        }


        public ApiResponse<List<BlogResponse>> GetBlogs()
        {
            var userBlogs = _context.BlogPosts.ToList();
            var blogsToReturn = _mapper.Map<List<BlogResponse>>(userBlogs);

            return new ApiResponse<List<BlogResponse>>
            {
                IsSucessFull = true,
                Message = "Blogs Fetched Sucessful",
                Payload = blogsToReturn
            };
        }

        public ApiResponse<int> ImportBlogs()
        {

            var client = new RestClient(_appSettings.BlogImportUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            IRestResponse response = client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                //sucessful Response

                var blogsImportResponse = JsonConvert.DeserializeObject<BlogImportResponse>(response.Content);
                var blogsFroImport = _mapper.Map<List<BlogPost>>(blogsImportResponse.data);
                List<BlogPost> blogsForDB = new List<BlogPost>();
                var existingBlogs = _context.BlogPosts.ToList();
                foreach (var item in blogsFroImport)
                {
                    var existBlog = existingBlogs.FirstOrDefault(x=>x.Title == item.Title && x.Description == item.Description);
                    if(existBlog == null)
                    {
                        blogsForDB.Add(item);
                    }
                }
                _context.BlogPosts.AddRange(blogsForDB);
                _context.SaveChanges();

                return new ApiResponse<int>
                {
                    IsSucessFull = true,
                    Message = blogsForDB.Count == 0 ? $"Blogs Import is up to Date" : $"{blogsForDB.Count} Imported Sucessfully",
                    Payload = blogsForDB.Count
                };
            }

            throw new AppException($"Blog Import Faied, please try again");

        }


        // helper methods

        private Account getAccount(int id)
        {
            var account = _context.Accounts.Find(id);
            if (account == null) throw new KeyNotFoundException("User for request not found");
            return account;
        }

        private bool isBlogAlreadyImported(BlogPost blogPost)
        {
           var existingBlog =  _context.BlogPosts.FirstOrDefault(x=>x.Title == blogPost.Title && x.Description == blogPost.Description);
            return existingBlog == null;
        }
    }
}
