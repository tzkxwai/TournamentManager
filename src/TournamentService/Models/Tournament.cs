
namespace TournamentService.Models;

public class Tournament
{
    // уникальный номер турнира

    public int Id { get; set; }

    // название турнира 

    public string Name { get; set; } = string.Empty;

    //дата и время

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // статус турнира 

    public bool IsFinished { get; set; } = false;

    // список айди игроков

    public List<int> PlayerIds { get; set; } = new();
}
