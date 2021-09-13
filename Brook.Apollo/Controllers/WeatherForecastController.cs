using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Brook.Apollo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _configuration;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }


        [HttpGet("apolloTest")]
        public string apolloTest()
        {
            return _configuration.GetValue("apollo-test","默认数据");
        }
    }
}
