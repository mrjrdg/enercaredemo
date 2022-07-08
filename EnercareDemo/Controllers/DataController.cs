using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace EnercareDemo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class DataController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _apiEndpoint;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public DataController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;

            if (configuration["ApiEndpoint"] == null)
                throw new ArgumentNullException("The Api Endpoint is missing from the configuration");

            _apiEndpoint = new Uri(configuration["WeatherApiEndpoint"], UriKind.Absolute);
        }

        [HttpGet]
        public async Task Get()
        {
            var accessToken = await _httpContextAccessor.HttpContext
                .GetTokenAsync("B2C", "access_token");

            var httpClient = _httpClientFactory
                .CreateClient();

            var request = new 
                HttpRequestMessage(HttpMethod.Get, 
                new Uri(_apiEndpoint, "Data"));

            request.Headers.Authorization = new 
                AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient
                .SendAsync(request);

            response.EnsureSuccessStatusCode();

            await response.Content.CopyToAsync(_httpContextAccessor.HttpContext.Response.Body);
        }
    }
}
