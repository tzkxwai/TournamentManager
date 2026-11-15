
using PlayerService.Models;
using Xunit;

namespace PlayerService.Tests;

public class PlayerTests
{
    // проверка автоматической установки даты

    [Fact]
    public void NewPlayer_ShouldHaveCreationDate()
    {
        var before = DateTime.UtcNow;

        var player = new Player
        {
            Name = "test_player"
        };

        var after = DateTime.UtcNow;

        Assert.InRange(player.CreatedAt, before, after);
    }

    // проверка получения рейтинга 1000


    [Fact]
    public void NewPlayer_ShouldHaveDefaultRating()
    {
        var player = new Player
        {
            Name = "test_player"
        };

        Assert.Equal(1000, player.Rating);
    }
}
