
namespace TournamentService.Models;

public class TournamentMatch
{
    // номер матча в базе данных
    public int Id { get; set; }

    // номер турнира
    public int TournamentId { get; set; }

    // номер раунда
    public int Round { get; set; }

    // первый участник
    public int? Player1Id { get; set; }

    // второй участник
    public int? Player2Id { get; set; }

    // победитель матча
    public int? WinnerId { get; set; }

    // отметка о начислении рейтинга
    public bool RatingUpdated { get; set; } = false;
}
