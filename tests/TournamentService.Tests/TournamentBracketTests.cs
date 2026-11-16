
using TournamentService.Services;
using Xunit;

namespace TournamentService.Tests;

public class TournamentBracketTests
{
    // проверяем создание сетки для четырёх игроков
    [Fact]
    public void FourPlayers_ShouldCreateTwoMatches()
    {
        var service = new TournamentBracketService();
        var players = new List<int> { 1, 2, 3, 4 };

        var matches = service.GenerateFirstRound(players);

        Assert.Equal(2, matches.Count);
        Assert.All(matches, m => Assert.Equal(1, m.Round));
    }

    // проверяем создание сетки для двух игроков
    [Fact]
    public void TwoPlayers_ShouldCreateOneMatch()
    {
        var service = new TournamentBracketService();
        var players = new List<int> { 1, 2 };

        var matches = service.GenerateFirstRound(players);

        Assert.Single(matches);
        Assert.Equal(1, matches[0].Round);
    }

    // проверяем удаление повторяющихся игроков
    [Fact]
    public void DuplicatePlayers_ShouldBeRemoved()
    {
        var service = new TournamentBracketService();
        var players = new List<int> { 1, 2, 2, 3 };

        var matches = service.GenerateFirstRound(players);

        var playerIds = matches
            .SelectMany(m => new[] { m.Player1Id, m.Player2Id })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        Assert.Equal(3, playerIds.Count);
        Assert.Equal(3, playerIds.Distinct().Count());
    }

    // проверяем запрет на турнир с одним игроком
    [Fact]
    public void OnePlayer_ShouldThrowException()
    {
        var service = new TournamentBracketService();
        var players = new List<int> { 1 };

        Assert.Throws<ArgumentException>(() =>
            service.GenerateFirstRound(players));
    }

    // Проверяем создание сетки для трёх игроков (BYE)
    [Fact]
    public void ThreePlayers_ShouldCreateTwoMatchesWithBye()
    {
        var service = new TournamentBracketService();
        var players = new List<int> { 1, 2, 3 };

        var matches = service.GenerateFirstRound(players);

        Assert.Equal(2, matches.Count);

        var playersInMatches = matches
            .SelectMany(m => new[] { m.Player1Id, m.Player2Id })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        Assert.Equal(3, playersInMatches.Count);
        Assert.Equal(3, playersInMatches.Distinct().Count());
    }

    // Проверяем запрет на создание сетки без игроков
    [Fact]
    public void EmptyPlayers_ShouldThrowException()
    {
        var service = new TournamentBracketService();
        var players = new List<int>();

        Assert.Throws<ArgumentException>(() =>
            service.GenerateFirstRound(players));
    }

}
