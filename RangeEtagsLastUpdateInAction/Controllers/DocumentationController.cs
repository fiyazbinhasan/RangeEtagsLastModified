using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RangeEtagsLastUpdateInAction.Controllers
{
    [Route("api/[controller]")]
    public class DocumentationController : Controller
    {
        [ResponseCache(Duration = 600)]
        [HttpGet("download")]
        public IActionResult Download()
        {
            return File("/assets/live.pdf", "application/pdf", "live.pdf", lastModified: DateTime.UtcNow.AddSeconds(-5), entityTag: new Microsoft.Net.Http.Headers.EntityTagHeaderValue("\"MyCalculatedEtagValue\""));
        }
    }
}
