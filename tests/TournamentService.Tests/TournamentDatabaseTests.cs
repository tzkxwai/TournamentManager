
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TournamentService.Data;
using TournamentService.Models;
using Xunit;

namespace TournamentService.Tests;

public class TournamentDatabaseTests
{
    // проверяем сохранение турнира в базе данных
    [Fact]
    public async Task AddTournament_ShouldSaveToDatabase()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TournamentDbContext>()
            .UseSqlite(connection)
            .Options;

        using var db = new TournamentDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var tournament = new Tournament
        {
            Name = "test_tournament",
            PlayerIds = new List<int> { 1, 2 }
        };

        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync();

        var saved = await db.Tournaments.FirstOrDefaultAsync();

        Assert.NotNull(saved);
        Assert.Equal("test_tournament", saved.Name);
        Assert.False(saved.IsFinished);
    }

    // проверяем сохранение матча в базе данных
    [Fact]
    public async Task AddMatch_ShouldSaveToDatabase()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TournamentDbContext>()
            .UseSqlite(connection)
            .Options;

        using var db = new TournamentDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var tournament = new Tournament
        {
            Name = "test_tournament",
            PlayerIds = new List<int> { 1, 2 }
        };

        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync();

        var match = new TournamentMatch
        {
            TournamentId = tournament.Id,
            Round = 1,
            Player1Id = 1,
            Player2Id = 2
        };

        db.Matches.Add(match);
        await db.SaveChangesAsync();

        var saved = await db.Matches.FirstOrDefaultAsync();

        Assert.NotNull(saved);
        Assert.Equal(tournament.Id, saved.TournamentId);
        Assert.Equal(1, saved.Round);
        Assert.Equal(1, saved.Player1Id);
        Assert.Equal(2, saved.Player2Id);
        Assert.Null(saved.WinnerId);
    }

    // проверяем сохранение победителя матча
    [Fact]
    public async Task UpdateWinner_ShouldSaveToDatabase()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TournamentDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var db = new TournamentDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();

            db.Matches.Add(new TournamentMatch
            {
                TournamentId = 1,
                Round = 1,
                Player1Id = 1,
                Player2Id = 2
            });

            await db.SaveChangesAsync();
        }

        using (var db = new TournamentDbContext(options))
        {
            var match = await db.Matches.FirstAsync();

            match.WinnerId = 1;
            await db.SaveChangesAsync();
        }

        using (var db = new TournamentDbContext(options))
        {
            var match = await db.Matches.FirstAsync();

            Assert.Equal(1, match.WinnerId);
        }
    }
}
