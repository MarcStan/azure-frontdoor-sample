using Microsoft.AspNetCore.Mvc;

namespace Frontdoor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "OK";
        }
    }
}
