using EnercareDemo2.Dto;
using EnercareDemo2.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace EnercareDemo2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {


        private readonly ILogger<AuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration;
        private readonly SignUpMagicLinkService _signUpMagicLinkService;
        private readonly UserMagicLinkInvitationHandler _userMagicLinkInvitationHandler;

        public AuthController(
            ILogger<AuthController> logger, 
            IHttpContextAccessor httpContextAccessor,
            ITokenAcquisition tokenAcquisition,
            HttpClient httpClient,
            IConfiguration configuration,
            SignUpMagicLinkService signUpMagicLinkService,
            UserMagicLinkInvitationHandler userMagicLinkInvitationHandler)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _tokenAcquisition = tokenAcquisition;
            _httpClient = httpClient;
            _configuration = configuration;
            _signUpMagicLinkService = signUpMagicLinkService;
            _userMagicLinkInvitationHandler = userMagicLinkInvitationHandler;
        }

        [HttpGet("login")]
        public ActionResult Login()
        {
            return Challenge(
                new AuthenticationProperties { RedirectUri = "/" }, 
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("getuser")]
        public async Task<IActionResult> GetUser()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var claims = ((ClaimsIdentity)User.Identity).Claims
                    .Select(c => new { type = c.Type, value = c.Value })
                    .ToArray();

                return Ok(new { isAuthenticated = true, claims = claims });
            }

            return Ok(new { isAuthenticated = false });
        }

        [HttpGet("SignInMagicLink")]
        public IActionResult SignInMagicLink([FromQuery] string id_token_hint)
        {
            var magic_link_auth = new 
                AuthenticationProperties { RedirectUri = "/" };

            magic_link_auth.Items.Add("id_token_hint", id_token_hint);

            string magic_link_policy = _configuration
                .GetSection("AzureAdB2CConfiguration")["MagicLinksPolicyId"];

            return Challenge(magic_link_auth, magic_link_policy);
        }

        [HttpGet("SignUpMagicLink")]
        public IActionResult SignUpMagicLink([FromQuery] string id_token_hint)
        {
            var invite_auth = new 
                AuthenticationProperties { RedirectUri = "/" };

            invite_auth.Items.Add("id_token_hint", id_token_hint);

            string invite_policy = _configuration
                .GetSection("AzureAdB2CConfiguration")["InvitationPolicyId"];

            return Challenge(invite_auth, invite_policy);
        }


        [HttpGet("test")]
        public async Task<ActionResult> Test()
        {

            if(User.Identity?.IsAuthenticated == true)
            {
                var accessToken = await _tokenAcquisition
                    .GetAccessTokenForUserAsync(scopes: new[] { "https://EnercareTest.onmicrosoft.com/api/api.read" },
                        authenticationScheme: OpenIdConnectDefaults.AuthenticationScheme);

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                _httpClient.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return Ok();
        }

        [HttpGet("SignUpMagicLinkValidation")]
        public async Task<IActionResult> SignUpMagicLinkValidation(string email, string contactId)
        {
            bool valid = true;

            // do validation work
            //....
            //....

            // Get redirect url
            var redirectUrl = valid ?
                _signUpMagicLinkService.GetRedirectUrlWithIdTokenHint(email, contactId) :
                "https://xxxxxxx:xxxx/errorpage";

            return Redirect(redirectUrl);
        }

        [HttpPost("SignInMagicLinkInput")]
        public async Task<IActionResult> PostAsync()
        {
            string input = null;

            // If not data came in, then return
            if (this.Request.Body == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict,
                    new AzureADB2CResponse("Request content is null", HttpStatusCode.Conflict));
            }

            // Read the input claims from the request body
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                input = await reader.ReadToEndAsync();
            }

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return StatusCode((int)HttpStatusCode.Conflict,
                    new AzureADB2CResponse("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into InputClaimsModel object
            var userInputClaimsForMagicLink = JsonSerializer
                .Deserialize<UserInputClaimsForMagicLink>(input);

            if (userInputClaimsForMagicLink == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict,
                    new AzureADB2CResponse("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            // Check input email address 
            if (string.IsNullOrEmpty(userInputClaimsForMagicLink.Email))
            {
                return StatusCode((int)HttpStatusCode.Conflict,
                    new AzureADB2CResponse("Email address is empty", HttpStatusCode.Conflict));
            }

            await _userMagicLinkInvitationHandler.SendEmailWithMagicLinkAsync(userInputClaimsForMagicLink);

            return Ok();
        }

        /*[Authorize]
        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return new SignOutResult("Auth0", new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            });
        }*/
    }
}