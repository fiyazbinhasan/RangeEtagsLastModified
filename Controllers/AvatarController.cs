using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace RangeEtagsLastModified.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvatarController : ControllerBase
    {
        private readonly ApplicationDbContext _contenxt;
        private readonly IMemoryCache _cache;

        public AvatarController(ApplicationDbContext context, IMemoryCache cache)
        {
            _contenxt = context;
            _cache = cache;
        }

        [ResponseCache(Duration = 30)]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_contenxt.Avatars);
        }

        [ResponseCache(Duration = 30)]
        [HttpGet("{id:guid}", Name = "AvatarGet")]
        public IActionResult Get(Guid id)
        {
            if (Request.Headers.ContainsKey("If-None-Match"))
            {
                var oldETag = Request.Headers["If-None-Match"].First();
                if (_cache.Get($"Avatar-{id}-{oldETag}") != null)
                {
                    return new StatusCodeResult(304);
                }
            }

            var avatar = _contenxt.Avatars.Find(id);
            if (avatar == null) return new NotFoundResult();

            var eTag = Convert.ToBase64String(avatar.RowVersion);

            _cache.Set($"Avatar-{id}-{eTag}", avatar);
            Response.Headers.Add("ETag", eTag);

            return Ok(avatar);
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile avatar)
        {
            try
            {
                var newAvatar = new Avatar()
                {
                    RowVersion = Guid.NewGuid().ToByteArray()
                };

                using (var memoryStream = new MemoryStream())
                {
                    await avatar.CopyToAsync(memoryStream);
                    newAvatar.File = memoryStream.ToArray();
                }

                _contenxt.Avatars.Add(newAvatar);

                if (await _contenxt.SaveChangesAsync() > 0)
                {
                    var url = Url.Link("AvatarGet", new { id = newAvatar.Id });
                    return Created(url, avatar);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return BadRequest("Could not add new Avatar.");
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, IFormFile avatar)
        {
            try
            {
                var avatarInDb = _contenxt.Avatars.Find(id);
                if (avatarInDb == null) return NotFound();

                if (Request.Headers.ContainsKey("If-Match"))
                {
                    var etag = Request.Headers["If-Match"].First();
                    if (etag != Convert.ToBase64String(avatarInDb.RowVersion))
                    {
                        return StatusCode((int)HttpStatusCode.PreconditionFailed);
                    }
                }

                using (var memoryStream = new MemoryStream())
                {
                    await avatar.CopyToAsync(memoryStream);
                    avatarInDb.File = memoryStream.ToArray();
                }

                avatarInDb.RowVersion = Guid.NewGuid().ToByteArray();

                var avatarModified = _contenxt.Update(avatarInDb);

                if (await _contenxt.SaveChangesAsync() > 0)
                {
                    var eTag = Convert.ToBase64String(avatarModified.Entity.RowVersion);
                    _cache.Set($"Avatar-{id}-{eTag}", avatarModified.Entity);

                    Response.Headers.Add("ETag", eTag);

                    return Ok(avatar);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return BadRequest("Could not update Avatar.");
        }

        [HttpGet("{id:guid}/DownloadAvatar")]
        public IActionResult DownloadAvatar(Guid id)
        {
            if (Request.Headers.ContainsKey("If-None-Match"))
            {
                var oldETag = Request.Headers["If-None-Match"].First();
                if (_cache.Get($"Avatar-{id}-{oldETag}") != null)
                {
                    return new StatusCodeResult(304);
                }
            }

            var avatar = _contenxt.Avatars.Find(id);
            if (avatar == null) return NotFound();
            
            var eTag = Convert.ToBase64String(avatar.RowVersion);

            _cache.Set($"Avatar-{id}-{eTag}", avatar);

            var entityTag = new EntityTagHeaderValue($"\"{eTag}\"");
            return File(avatar.File, "image/png", "avatar.png", lastModified: DateTime.UtcNow.AddSeconds(-5), entityTag: entityTag);
        }
    }
}
