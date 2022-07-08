using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace EnercareAzureB2cWebApi.Controllers
{

    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    
    [Authorize]
    [ApiController]
    [RequiredScope("api.read")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("items")]
        public async Task<IActionResult> Get()
        {
            return Ok(new List<Item>
            {
                new Item
                {
                    Name = "P1",
                    Description = "Iron"
                },
                new Item
                {
                    Name = "P2",
                    Description = "Steel"
                },
                new Item
                {
                    Name = "P3",
                    Description = "Gold"
                },
            });
        }

    }
}