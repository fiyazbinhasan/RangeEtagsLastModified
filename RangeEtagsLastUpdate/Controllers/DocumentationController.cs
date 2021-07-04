using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;

namespace RangeEtagsLastUpdate.Controllers
{
    [Route("api/[controller]")]
    public class DocumentationController : ControllerBase
    {
        [HttpGet("download")]
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
