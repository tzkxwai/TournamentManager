
using Microsoft.EntityFrameworkCore;
using TournamentService.Models;

namespace TournamentService.Data;

public class TournamentDbContext : DbContext
{
    // настройки подключения к базе данных
    public TournamentDbContext(DbContextOptions<TournamentDbContext> options)
        : base(options)
    {
    }

    // таблица турниров
    public DbSet<Tournament> Tournaments { get; set; }

    // таблица матчей
    public DbSet<TournamentMatch> Matches { get; set; }
}
