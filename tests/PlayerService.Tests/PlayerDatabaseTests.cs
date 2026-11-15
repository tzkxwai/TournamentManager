
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlayerService.Data;
using PlayerService.Models;
using Xunit;

namespace PlayerService.Tests;

public class PlayerDatabaseTests
{
    [Fact]
    public async Task AddPlayer_ShouldSavePlayerToDatabase()
    {
        // временначя бд

        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseSqlite(connection)
            .Options;

        using var db = new PlayerDbContext(options);

        await db.Database.EnsureCreatedAsync();

        // +1

        var player = new Player
        {
            Name = "test_player"
        };

        db.Players.Add(player);
        await db.SaveChangesAsync();

        // проверка сохранения

        var savedPlayer = await db.Players
            .FirstOrDefaultAsync(p => p.Name == "test_player");

        Assert.NotNull(savedPlayer);
        Assert.Equal(1000, savedPlayer.Rating);
    }

    [Fact]
    public async Task GetPlayer_ShouldReadPlayerFromDatabase()
    {
        // временная бд

        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseSqlite(connection)
            .Options;

        // сохранение игрока через 1 контекст

        using (var db = new PlayerDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();

            db.Players.Add(new Player
            {
                Name = "burmalda"
            });

            await db.SaveChangesAsync();
        }

        // чтение игрока через 2 контекст

        using (var db = new PlayerDbContext(options))
        {
            var player = await db.Players
                .FirstOrDefaultAsync(p => p.Name == "burmalda");

            Assert.NotNull(player);
            Assert.Equal("burmalda", player.Name);
            Assert.Equal(1000, player.Rating);
        }
    }

    [Fact]
    public async Task UpdatePlayer_ShouldSaveNewName()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var db = new PlayerDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();

            db.Players.Add(new Player
            {
                Name = "old_name"
            });

            await db.SaveChangesAsync();
        }

        // замена имени

        using (var db = new PlayerDbContext(options))
        {
            var player = await db.Players.FirstAsync();

            player.Name = "new_name";
            await db.SaveChangesAsync();
        }

        // проверка изменений через новый контекст

        using (var db = new PlayerDbContext(options))
        {
            var player = await db.Players.FirstAsync();

            Assert.Equal("new_name", player.Name);
            Assert.Equal(1000, player.Rating);
        }
    }

    [Fact]
    public async Task DeletePlayer_ShouldRemovePlayerFromDatabase()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PlayerDbContext>()
            .UseSqlite(connection)
            .Options;

        // новый игрок

        using (var db = new PlayerDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();

            db.Players.Add(new Player
            {
                Name = "delete_test"
            });

            await db.SaveChangesAsync();
        }

        // удаление нового игрока

        using (var db = new PlayerDbContext(options))
        {
            var player = await db.Players.FirstAsync();

            db.Players.Remove(player);
            await db.SaveChangesAsync();
        }

        // проверка удаления

        using (var db = new PlayerDbContext(options))
        {
            var player = await db.Players
                .FirstOrDefaultAsync(p => p.Name == "delete_test");

            Assert.Null(player);
            Assert.Empty(await db.Players.ToListAsync());
        }
    }


}
