using Microsoft.AspNetCore.Mvc;

namespace RealTime_APIDev.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
