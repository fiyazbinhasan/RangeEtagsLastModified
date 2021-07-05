using Microsoft.AspNetCore.Mvc;

namespace RangeEtagsLastModified.Controllers
{
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        [HttpGet("download")]
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Client)]
        public IActionResult Download()
        {
            return File("/assets/guitar.mp3", 
                "audio/mpeg", 
                "guitar.mp3");
        }
    }
}
