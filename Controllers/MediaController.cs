using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;

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
                "guitar.mp3",
                lastModified: DateTime.UtcNow.AddSeconds(-5),
                entityTag: new EntityTagHeaderValue($"\"{Convert.ToBase64String(Guid.NewGuid().ToByteArray())}\""),
                enableRangeProcessing: true);
        }
    }
}
