
using Microsoft.EntityFrameworkCore;
using PlayerService.Models;

namespace PlayerService.Data
{
    public class PlayerDbContext : DbContext
    {
        public PlayerDbContext(DbContextOptions<PlayerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; } = null!;
    }
}
