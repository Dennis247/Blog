namespace Blog.api.Controllers;

using Blog.api.Services;
using Blog.Domain.Enums;
using Blog.Domain.Models.Accounts;
using Blog.Domain.Models.Response;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountsController : BaseController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public IActionResult Authenticate(AuthenticateRequest model)
    {
        var response = _accountService.Authenticate(model, ipAddress());
        return Ok(response);
    }


    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest model)
    {
        var response =  _accountService.Register(model, Request.Headers["origin"]);
        return Ok(response);
    }



   // [Authorize(Role.Admin)]
    [HttpGet]
    public IActionResult GetAllUsers()
    {
        var accounts = _accountService.GetAllUsers();
        return Ok(accounts);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        // users can get their own account and admins can get any account
        if (id != Account.Id && Account.Role != Role.Admin)
            return Unauthorized(new ApiResponse<string>{ 
                Message = "Unauthorized",
                IsSucessFull=false,
                Payload = null });

        var account = _accountService.GetById(id);
        return Ok(account);
    }

    //[Authorize(Role.Admin)]
    [HttpPost]
    public IActionResult Create(CreateRequest model)
    {
        var response = _accountService.Create(model);
        return Ok(response);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, UpdateRequest model)
    {
        // users can update their own account and admins can update any account
        if (id != Account.Id && Account.Role != Role.Admin)
            return Unauthorized(new ApiResponse<string>
            {
                Message = "Unauthorized",
                IsSucessFull = false,
                Payload = null
            });


        var account = _accountService.Update(id, model);
        return Ok(account);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        // users can delete their own account and admins can delete any account
        if (id != Account.Id && Account.Role != Role.Admin)
            return  Unauthorized(new ApiResponse<string>
            {
                Message = "Unauthorized",
                IsSucessFull = false,
                Payload = null
            });

        _accountService.Delete(id);
        return Ok(new { message = "Account deleted successfully" });
    }



    private string ipAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }

  
}