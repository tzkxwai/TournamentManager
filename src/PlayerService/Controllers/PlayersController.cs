
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerService.Data;
using PlayerService.Models;

namespace PlayerService.Controllers
{
    [ApiController]
    [Route("api/players")]
    public class PlayersController : ControllerBase
    {
        private readonly PlayerDbContext _context;

        public PlayersController(PlayerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            var players = await _context.Players.ToListAsync();

            return Ok(players);
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayer(Player player)
        {
            player.Id = 0;
            player.Rating = 1000;
            player.CreatedAt = DateTime.UtcNow;

            _context.Players.Add(player);

            await _context.SaveChangesAsync();

            return Ok(player);
        }
    }
}
