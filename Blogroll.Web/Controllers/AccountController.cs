using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Blogroll.Web.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogroll.Web.Controllers
{
    public class AccountController : Controller
    {
        public AccountController(IAuthenticating auth)
        {
            _auth = auth;
        }

        private readonly IAuthenticating _auth;

        private string SigninPath() => "/Account/Login";

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login() => View();

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignIn([FromForm] string password)
        {
            if (!_auth.Authenticated(password))
            {
                return Redirect(SigninPath());
            }
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(
                    new List<Claim>{new Claim(ClaimTypes.Name, "Owner")}, 
                    CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties
                {
                    IsPersistent = true
                });
            return Redirect("/");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(SigninPath());
        }
    }
}