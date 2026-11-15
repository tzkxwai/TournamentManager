
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

            return Ok(player);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlayer(int id, Player updatedPlayer)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

            player.Name = updatedPlayer.Name;

            await _context.SaveChangesAsync();

            return Ok(player);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

            _context.Players.Remove(player);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
