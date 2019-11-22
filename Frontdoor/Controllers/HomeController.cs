using Microsoft.AspNetCore.Mvc;
using System;

namespace Frontdoor.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return Environment.GetEnvironmentVariable("REGION_NAME") ?? "UNKNOWN AZURE DATA CENTER";
        }
    }
}
