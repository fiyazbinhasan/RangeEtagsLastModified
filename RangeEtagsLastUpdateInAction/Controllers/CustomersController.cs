using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace RangeEtagsLastUpdateInAction.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private ApplicationDbContext _contenxt;
        private IMemoryCache _cache;

        public CustomersController(ApplicationDbContext context, IMemoryCache cache)
        {
            _contenxt = context;
            _cache = cache;
        }

        [ResponseCache(Duration = 30)]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_contenxt.Customers);
        }

        [ResponseCache(Duration = 30)]
        [HttpGet("{id:guid}", Name = "CustomerGet")]
        public IActionResult Get(Guid id)
        {
            if (Request.Headers.ContainsKey("If-None-Match"))
            {
                var oldETag = Request.Headers["If-None-Match"].First();
                if (_cache.Get($"Customer-{id}-{oldETag}") != null)
                {
                    return new StatusCodeResult(304);
                }
            }

            var customer = _contenxt.Customers.Find(id);
            if (customer == null) return new NotFoundResult();

            var eTag = Convert.ToBase64String(customer.RowVersion);

            _cache.Set($"Customer-{id}-{eTag}", customer);
            Response.Headers.Add("ETag", eTag);

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            try
            {
                _contenxt.Customers.Add(customer);

                if (await _contenxt.SaveChangesAsync() > 0)
                {
                    var url = Url.Link("CustomerGet", new { id = customer.Id });
                    return Created(url, customer);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return BadRequest("Could not add new Customer.");
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Customer customer)
        {
            try
            {
                var customerInDb = _contenxt.Customers.Find(id);
                if (customerInDb == null) return NotFound();

                if (Request.Headers.ContainsKey("If-Match"))
                {
                    var etag = Request.Headers["If-Match"].First();
                    if (etag != Convert.ToBase64String(customerInDb.RowVersion))
                    {
                        return StatusCode((int)HttpStatusCode.PreconditionFailed);
                    }
                }

                customerInDb.Name = customer.Name;
                customerInDb.Email = customer.Email;
                customerInDb.Avatar = customer.Avatar;

                var customerModified = _contenxt.Update(customerInDb);

                if (await _contenxt.SaveChangesAsync() > 0)
                {
                    var eTag = Convert.ToBase64String(customerModified.Entity.RowVersion);
                    _cache.Set($"Customer-{id}-{eTag}", customerModified.Entity);

                    Response.Headers.Add("ETag", eTag);

                    return Ok(customer);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return BadRequest("Could not update Customer.");
        }

        [HttpGet("{id:guid}/DownloadAvatar")]
        public IActionResult DownloadAvatar(Guid id)
        {
            if (Request.Headers.ContainsKey("If-None-Match"))
            {
                var oldETag = Request.Headers["If-None-Match"].First();
                if (_cache.Get($"Customer-{id}-{oldETag}") != null)
                {
                    return new StatusCodeResult(304);
                }
            }

            var customer = _contenxt.Customers.Find(id);
            if (customer == null) return NotFound();
            
            var eTag = Convert.ToBase64String(customer.RowVersion);

            _cache.Set($"Customer-{id}-{eTag}", customer);

            var entityTag = new EntityTagHeaderValue($"\"{eTag}\"");
            return File(customer.Avatar, "image/png", "avatar.png", lastModified: DateTime.UtcNow.AddSeconds(-5), entityTag: entityTag);
        }
    }
}
