
using TournamentService.Models;

namespace TournamentService.Services;

public class TournamentBracketService
{
    // создаём первый раунд олимпийской сетки
    public List<TournamentMatch> GenerateFirstRound(List<int> playerIds)
    {
        // убираем повторяющиеся ID игроков
        var players = playerIds.Distinct().ToList();

        // для турнира нужно минимум два игрока
        if (players.Count < 2)
            throw new ArgumentException("Нужно минимум два игрока");

        var matches = new List<TournamentMatch>();

        // определяем размер сетки: 2, 4, 8, 16 и так далее
        var bracketSize = 1;

        while (bracketSize < players.Count)
            bracketSize *= 2;

        // создаём пары первого раунда
        for (int i = 0; i < bracketSize / 2; i++)
        {
            matches.Add(new TournamentMatch
            {
                Round = 1,
                Player1Id = i < players.Count ? players[i] : null,
                Player2Id = bracketSize - 1 - i < players.Count
                    ? players[bracketSize - 1 - i]
                    : null
            });
        }

        return matches;
    }
}
