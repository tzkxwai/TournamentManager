
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentService.Data;
using TournamentService.Models;
using TournamentService.Services;

namespace TournamentService.Controllers;

[ApiController]
[Route("api/tournaments")]
public class TournamentsController : ControllerBase
{
    private readonly TournamentDbContext _db;
    private readonly TournamentBracketService _bracketService;

    // доступ к бд и генератору сетки
    public TournamentsController(
        TournamentDbContext db,
        TournamentBracketService bracketService)
    {
        _db = db;
        _bracketService = bracketService;
    }

    // список всех турниров
    [HttpGet]
    public async Task<IActionResult> GetTournaments()
    {
        return Ok(await _db.Tournaments.ToListAsync());
    }

    // получаем турнир по айди
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTournament(int id)
    {
        var tournament = await _db.Tournaments.FindAsync(id);

        if (tournament == null)
            return NotFound();

        return Ok(tournament);
    }

    // создаём новый турнир
    [HttpPost]
    public async Task<IActionResult> AddTournament(Tournament tournament)
    {
        tournament.Id = 0;
        tournament.CreatedAt = DateTime.UtcNow;
        tournament.IsFinished = false;

        _db.Tournaments.Add(tournament);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTournament),
            new { id = tournament.Id },
            tournament);
    }

    // получаем или создаём сетку турнира
    [HttpGet("{id}/bracket")]
    public async Task<IActionResult> GetBracket(int id)
    {
        var tournament = await _db.Tournaments.FindAsync(id);

        if (tournament == null)
            return NotFound();

        // проверяем, есть ли сохранённые матчи
        var matches = await _db.Matches
            .Where(m => m.TournamentId == id)
            .OrderBy(m => m.Round)
            .ThenBy(m => m.Id)
            .ToListAsync();

        if (matches.Count > 0)
            return Ok(matches);

        // проверяем количество участников
        if (tournament.PlayerIds.Distinct().Count() < 2)
            return BadRequest("Для сетки нужно минимум два игрока");

        // создаём пары первого раунда
        matches = _bracketService.GenerateFirstRound(tournament.PlayerIds);

        // привязываем матчи к турниру
        foreach (var match in matches)
            match.TournamentId = id;

        // сохраняем матчи в базе данных
        _db.Matches.AddRange(matches);
        await _db.SaveChangesAsync();

        return Ok(matches);
    }

    // записываем победителя матча
    [HttpPut("{id}/matches/{matchId}/winner")]
    public async Task<IActionResult> SetMatchWinner(
        int id, int matchId, [FromBody] int winnerId)
    {
        // ищем турнир
        var tournament = await _db.Tournaments.FindAsync(id);

        if (tournament == null)
            return NotFound("Турнир не найден");

        // запрещаем менять завершённый турнир
        if (tournament.IsFinished)
            return BadRequest("Турнир уже завершён");

        // ищем матч в базе данных
        var match = await _db.Matches
            .FirstOrDefaultAsync(m =>
                m.Id == matchId && m.TournamentId == id);

        if (match == null)
            return NotFound("Матч не найден");

        // запрещаем менять записанного победителя
        if (match.WinnerId != null)
            return BadRequest("Победитель этого матча уже определён");

        // проверяем, что игрок участвует в матче
        if (winnerId != match.Player1Id && winnerId != match.Player2Id)
            return BadRequest("Игрок не участвует в этом матче");

        // сохраняем победителя
        match.WinnerId = winnerId;

        // получаем все матчи турнира
        var allMatches = await _db.Matches
            .Where(m => m.TournamentId == id)
            .ToListAsync();

        // определяем последний раунд
        var lastRound = allMatches.Max(m => m.Round);

        var finalMatches = allMatches
            .Where(m => m.Round == lastRound)
            .ToList();

        // проверяем, завершён ли финал
        if (finalMatches.Count == 1 &&
            finalMatches.All(m => m.WinnerId != null))
        {
            tournament.IsFinished = true;
        }

        await _db.SaveChangesAsync();

        return Ok(match);
    }

    // создаём следующий раунд турнира
    [HttpPost("{id}/next-round")]
    public async Task<IActionResult> GenerateNextRound(int id)
    {
        var tournament = await _db.Tournaments.FindAsync(id);

        if (tournament == null)
            return NotFound("Турнир не найден");

        // запрещаем создавать раунды завершённого турнира
        if (tournament.IsFinished)
            return BadRequest("Турнир уже завершён");

        // получаем все матчи турнира
        var matches = await _db.Matches
            .Where(m => m.TournamentId == id)
            .ToListAsync();

        if (matches.Count == 0)
            return BadRequest("Сетка турнира ещё не создана");

        // определяем последний раунд
        var currentRound = matches.Max(m => m.Round);

        var roundMatches = matches
            .Where(m => m.Round == currentRound)
            .OrderBy(m => m.Id)
            .ToList();

        // проверяем, что все матчи завершены
        if (roundMatches.Any(m => m.WinnerId == null))
            return BadRequest("Не все матчи текущего раунда завершены");

        // получаем победителей
        var winners = roundMatches
            .Select(m => m.WinnerId!.Value)
            .ToList();

        // проверяем, определён ли чемпион
        if (winners.Count == 1)
            return BadRequest("Финал уже завершён");

        // создаём следующий раунд
        var nextMatches = _bracketService.GenerateFirstRound(winners);

        foreach (var match in nextMatches)
        {
            match.TournamentId = id;
            match.Round = currentRound + 1;
        }

        // сохраняем новый раунд
        _db.Matches.AddRange(nextMatches);
        await _db.SaveChangesAsync();

        return Ok(nextMatches);
    }
}
