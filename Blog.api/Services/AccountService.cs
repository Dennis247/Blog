namespace Blog.api.Services;
using AutoMapper;
using BCrypt.Net;
using Blog.api.Authorization;
using Blog.api.Helpers;
using Blog.Domain.Context;
using Blog.Domain.Entities;
using Blog.Domain.Enums;
using Blog.Domain.Models.Accounts;
using Blog.Domain.Models.Response;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public interface IAccountService
{
    ApiResponse<AuthenticateResponse> Authenticate(AuthenticateRequest model, string ipAddress);
    ApiResponse<AccountResponse> Create(CreateRequest model);
    ApiResponse<bool> Delete(int id);
    ApiResponse<IEnumerable<AccountResponse>> GetAllUsers();
    ApiResponse<AccountResponse> GetById(int id);
    ApiResponse<AuthenticateResponse> Register(RegisterRequest model, string origin);
    ApiResponse<AccountResponse> Update(int id, UpdateRequest model);
}

public class AccountService : IAccountService
{
    private readonly DataContext _context;
    private readonly IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;

    public AccountService(
        DataContext context,
        IJwtUtils jwtUtils,
        IMapper mapper,
        IOptions<AppSettings> appSettings)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }

    public ApiResponse<AuthenticateResponse> Authenticate(AuthenticateRequest model, string ipAddress)
    {
        var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

        // validate
        if (account == null ||  !BCrypt.Verify(model.Password, account.PasswordHash))
            throw new AppException("Email or password is incorrect");

        // authentication successful so generate jwt and refresh tokens
        var jwtToken = _jwtUtils.GenerateJwtToken(account);

        // save changes to db
        _context.Update(account);
        _context.SaveChanges();

        var response = _mapper.Map<AuthenticateResponse>(account);
        response.JwtToken = jwtToken;
        return new ApiResponse<AuthenticateResponse>
        {
            IsSucessFull = true,
            Message = "Login Sucessful",
            Payload = response
        };
    }

    public ApiResponse<AuthenticateResponse> Register(RegisterRequest model, string origin)
    {
        // validate
        if (_context.Accounts.Any(x => x.Email == model.Email))
        {
            throw new AppException("Email already exist");
        }

        // map model to new account object
        var account = _mapper.Map<Account>(model);

        // first registered account is an admin
        var isFirstAccount = _context.Accounts.Count() == 0;
        account.Role = isFirstAccount ? Role.Admin : Role.User;
        account.Created = DateTime.UtcNow;

        // hash password
        account.PasswordHash = BCrypt.HashPassword(model.Password);


        // authentication successful so generate jwt and refresh tokens
 

        // save account
        _context.Accounts.Add(account);
        _context.SaveChanges();

        var jwtToken = _jwtUtils.GenerateJwtToken(account);

        var response = _mapper.Map<AuthenticateResponse>(account);
        response.JwtToken = jwtToken;

        return new ApiResponse<AuthenticateResponse>
        {
            Payload = response,
            IsSucessFull = true,
            Message = "Registration Sucessful",

        };
    }


    public ApiResponse<IEnumerable<AccountResponse>> GetAllUsers()
    {
        var accounts = _context.Accounts;
        var usersToResurn = _mapper.Map<IList<AccountResponse>>(accounts);
        return new ApiResponse<IEnumerable<AccountResponse>>
        {
            IsSucessFull = true,
            Message = "Sucessful",
            Payload = usersToResurn
        };
    }

    public ApiResponse<AccountResponse> GetById(int id)
    {
        var account = getAccount(id);
        var accountToResurn = _mapper.Map<AccountResponse>(account);
        return new ApiResponse<AccountResponse>
        {
            IsSucessFull = true,
            Message = "Sucessful",
            Payload = accountToResurn
        };
    }

    public ApiResponse<AccountResponse> Create(CreateRequest model)
    {
        // validate
        if (_context.Accounts.Any(x => x.Email == model.Email))
            throw new AppException($"Email '{model.Email}' is already registered");

        // map model to new account object
        var account = _mapper.Map<Account>(model);
        account.Created = DateTime.UtcNow;
        account.Verified = DateTime.UtcNow;

        // hash password
        account.PasswordHash = BCrypt.HashPassword(model.Password);

        // save account
        _context.Accounts.Add(account);
        _context.SaveChanges();

        var accountResponse = _mapper.Map<AccountResponse>(account);
        return new ApiResponse<AccountResponse>
        {
            IsSucessFull = true,
            Payload = accountResponse,
            Message = "Sucessful"
        };
    }

    public ApiResponse<AccountResponse> Update(int id, UpdateRequest model)
    {
        var account = getAccount(id);

        // validate
        if (account == null)
            throw new AppException($"Account does not exist");



        // copy model to account and save
        _mapper.Map(model, account);
        account.Updated = DateTime.UtcNow;
        _context.Accounts.Update(account);
        _context.SaveChanges();

        var updatedProfile = _mapper.Map<AccountResponse>(account);
        return new ApiResponse<AccountResponse>
        {
            IsSucessFull = true,
            Payload = updatedProfile,
            Message = "User Update Sucessful"
        };

    }

    public ApiResponse<bool> Delete(int id)
    {
        var account = getAccount(id);
        _context.Accounts.Remove(account);
        _context.SaveChanges();

        return new ApiResponse<bool>
        {
            IsSucessFull = true,
            Payload = true,
            Message = "Delete Sucessful"
        };
    }

    // helper methods

    private Account getAccount(int id)
    {
        var account = _context.Accounts.Find(id);
        if (account == null) throw new KeyNotFoundException("Account not found");
        return account;
    }


}