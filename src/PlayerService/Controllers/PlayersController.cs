
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

        // получаем всех игроков
        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            var players = await _context.Players
                .OrderByDescending(p => p.Rating)
                .ToListAsync();

            return Ok(players);
        }

        // получаем игрока по ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
                return NotFound();

            return Ok(player);
        }

        // создаём игрока
        [HttpPost]
        public async Task<IActionResult> AddPlayer(Player player)
        {
            if (string.IsNullOrWhiteSpace(player.Name))
                return BadRequest("Введите имя игрока");

            player.Id = 0;
            player.Name = player.Name.Trim();
            player.Rating = 1000;
            player.CreatedAt = DateTime.UtcNow;

            _context.Players.Add(player);

            await _context.SaveChangesAsync();

            return Ok(player);
        }

        // изменяем имя игрока
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlayer(
            int id, Player updatedPlayer)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(updatedPlayer.Name))
                return BadRequest("Введите имя игрока");

            player.Name = updatedPlayer.Name.Trim();

            await _context.SaveChangesAsync();

            return Ok(player);
        }

        // пересчитываем рейтинг после матча
        [HttpPost("elo-result")]
        public async Task<IActionResult> UpdateElo(
            [FromBody] EloResultRequest request)
        {
            if (request.WinnerId == request.LoserId)
                return BadRequest("Игроки должны отличаться");

            var winner = await _context.Players.FindAsync(
                request.WinnerId);

            var loser = await _context.Players.FindAsync(
                request.LoserId);

            if (winner == null || loser == null)
                return NotFound("Игрок не найден");

            const int kFactor = 32;

            double expectedWinner =
                1.0 / (1.0 + Math.Pow(
                    10,
                    (loser.Rating - winner.Rating) / 400.0));

            int ratingChange = (int)Math.Round(
                kFactor * (1.0 - expectedWinner),
                MidpointRounding.AwayFromZero);

            winner.Rating += ratingChange;
            loser.Rating -= ratingChange;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Winner = winner.Name,
                WinnerRating = winner.Rating,
                Loser = loser.Name,
                LoserRating = loser.Rating,
                RatingChange = ratingChange
            });
        }

        // удаляем игрока
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
                return NotFound();

            _context.Players.Remove(player);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // данные для пересчёта рейтинга
    public class EloResultRequest
    {
        public int WinnerId { get; set; }
        public int LoserId { get; set; }
    }
}
