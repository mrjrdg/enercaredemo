using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnercareDemo.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpGet("login")]
        public ActionResult Login(string returnUrl)
        {
            return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = returnUrl
            });
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return new SignOutResult("Auth0", new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            });
        }

        [HttpGet("getuser")]
        public async Task<IActionResult> GetUser()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var claims = ((ClaimsIdentity)this.User.Identity).Claims.Select(c =>
                                new { type = c.Type, value = c.Value })
                                .ToArray();

                return Ok(new { isAuthenticated = true, claims = claims });
            }

            return Ok(new { isAuthenticated = false });
        }
    }
}