
using Microsoft.AspNetCore.Mvc;
using PlayerService.Models;

namespace PlayerService.Controllers
{
    [ApiController]
    [Route("api/players")]
    public class PlayersController : ControllerBase
    {
        private static readonly List<Player> Players = new();

        [HttpGet]
        public IActionResult GetPlayers()
        {
            return Ok(Players);
        }

        [HttpPost]
        public IActionResult AddPlayer(Player player)
        {
            player.Id = Players.Count + 1;
            Players.Add(player);

            return Ok(player);
        }
    }
}
