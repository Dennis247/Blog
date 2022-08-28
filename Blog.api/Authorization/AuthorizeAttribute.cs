namespace WebApi.Authorization;

using Blog.Domain.Entities;
using Blog.Domain.Enums;
using Blog.Domain.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly IList<Role> _roles;

    public AuthorizeAttribute(params Role[] roles)
    {
        _roles = roles ?? new Role[] { };
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // skip authorization if action is decorated with [AllowAnonymous] attribute
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
            return;

        // authorization
        var account = (Account)context.HttpContext.Items["Account"];
        if (account == null || (_roles.Any() && !_roles.Contains(account.Role)))
        {
            var responseModel = new ApiResponse<string>
            {
                Message = "Unauthorized",
                IsSucessFull = false,
                Payload = null
            };
       
            // not logged in or role not authorized
            context.Result = new JsonResult(responseModel) 
            { StatusCode = StatusCodes.Status401Unauthorized };
          
        }
    }
}