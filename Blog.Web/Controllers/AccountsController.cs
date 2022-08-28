using AspNetCoreHero.ToastNotification.Abstractions;
using Blog.Domain.Models.Accounts;
using Blog.Domain.Models.Response;
using Blog.Web.Models;
using Blog.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Blog.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly IHttpServices _httpServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSettings _appSettings;

        public AccountsController(INotyfService notyf, IOptions<AppSettings> appSettings,
            IHttpServices httpServices, IHttpContextAccessor httpContextAccessor)
        {
            _notyf = notyf;
            _httpServices = httpServices;
            _appSettings = appSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterRequest());
        }


        [HttpPost]
        public IActionResult Register(RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _notyf.Error(message);
                return View(registerRequest);
            }

            string url = $"{_appSettings.ApiUrl}/Accounts/Register";
            var payload = JsonConvert.SerializeObject(registerRequest);
            var dataResponse = _httpServices.Post(url, payload);
            var registerResult = JsonConvert.DeserializeObject<ApiResponse<AuthenticateResponse>>(dataResponse.Data);
            if (dataResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //store logged in user in a session
                string serializedProfile = JsonConvert.SerializeObject(registerResult.Payload);
                _httpContextAccessor.HttpContext.Session.SetString("userProfile", serializedProfile);
                _httpContextAccessor.HttpContext.Session.SetString("JWToken", registerResult.Payload.JwtToken);

                _notyf.Success("Registration Sucessful");

                return RedirectToAction("Index", "Home");
            }
            _notyf.Error(registerResult.Message);
            return View(registerRequest);
        }



        [HttpGet]
        public IActionResult Login()
        {
            return View(new AuthenticateRequest());
        }


        [HttpPost]
        public IActionResult Login(AuthenticateRequest authenticateRequest)
        {
            if (!ModelState.IsValid)
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _notyf.Error(message);
                return View(authenticateRequest);
            }
            string url = $"{_appSettings.ApiUrl}/Accounts/authenticate";
            var payload = JsonConvert.SerializeObject(authenticateRequest);
            var dataResponse = _httpServices.Post(url, payload);
            var loginResult = JsonConvert.DeserializeObject<ApiResponse<AuthenticateResponse>>(dataResponse.Data);
            if (dataResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //store logged in user in a session
                string serializedProfile = JsonConvert.SerializeObject(loginResult.Payload);
                _httpContextAccessor.HttpContext.Session.SetString("userProfile", serializedProfile);
                _httpContextAccessor.HttpContext.Session.SetString("JWToken", loginResult.Payload.JwtToken);

                _notyf.Success(loginResult.Message);

                return RedirectToAction("Index", "Blogs");
            }
            _notyf.Error(loginResult.Message);
            return View(authenticateRequest);
        }

        [HttpPost]
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Blogs");
         
        }
    }
}
