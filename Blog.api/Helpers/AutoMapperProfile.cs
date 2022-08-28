namespace Blog.api.Helpers;
using AutoMapper;
using Blog.Domain.Entities;
using Blog.Domain.Models.Accounts;
using Blog.Domain.Models.Blogs;

public class AutoMapperProfile : Profile
{
    // mappings between model and entity objects
    public AutoMapperProfile()
    {
        CreateMap<Account, AccountResponse>();

        CreateMap<Account, AuthenticateResponse>();

        CreateMap<RegisterRequest, Account>();

        CreateMap<CreateRequest, Account>();

        CreateMap<UpdateRequest, Account>();

        CreateMap<CreateBlog, BlogPost>();

        CreateMap<BlogPost, BlogResponse>();

        CreateMap<BlogImport, BlogPost>()
          .ForMember(dest => dest.PublicationDate, opt => opt.MapFrom(src => DateTime.Parse(src.publication_date)));

    }
}