using Microsoft.EntityFrameworkCore;

namespace RangeEtagsLastUpdate
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Avatar> Avatars { get; set; }
    }
}
