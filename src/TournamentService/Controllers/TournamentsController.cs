
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
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
    private readonly IHttpClientFactory _httpClientFactory;

    public TournamentsController(
        TournamentDbContext db,
        TournamentBracketService bracketService,
        IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _bracketService = bracketService;
        _httpClientFactory = httpClientFactory;
    }

    // проверяем завершение турнира
    private static bool IsTournamentFinished(
        List<TournamentMatch> matches)
    {
        if (matches.Count == 0)
            return false;

        int lastRound = matches.Max(m => m.Round);

        var finalMatches = matches
            .Where(m => m.Round == lastRound)
            .ToList();

        return finalMatches.Count == 1 &&
               finalMatches[0].WinnerId != null;
    }

    // автоматически определяем победителей свободного прохода
    private static bool SetAutomaticWinners(
        List<TournamentMatch> matches)
    {
        bool changed = false;

        foreach (var match in matches)
        {
            if (match.Player1Id != null &&
                match.Player2Id == null &&
                match.WinnerId == null)
            {
                match.WinnerId = match.Player1Id;
                changed = true;
            }
        }

        return changed;
    }

    // обновляем статус турнира
    private async Task UpdateTournamentStatus(
        Tournament tournament)
    {
        var matches = await _db.Matches
            .Where(m => m.TournamentId == tournament.Id)
            .ToListAsync();

        tournament.IsFinished =
            IsTournamentFinished(matches);
    }

    // обновляем рейтинг игроков через PlayerService
    private async Task<(bool Success, string Error)> UpdatePlayerRatings(
        int winnerId,
        int loserId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PlayerApi");

            var response = await client.PostAsJsonAsync(
                "api/players/elo-result",
                new
                {
                    WinnerId = winnerId,
                    LoserId = loserId
                });

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                return (false, error);
            }

            return (true, "");
        }
        catch (HttpRequestException ex)
        {
            return (false, ex.Message);
        }
    }

    // список всех турниров
    [HttpGet]
    public async Task<IActionResult> GetTournaments()
    {
        var tournaments = await _db.Tournaments.ToListAsync();

        foreach (var tournament in tournaments)
        {
            await UpdateTournamentStatus(tournament);
        }

        await _db.SaveChangesAsync();

        return Ok(tournaments);
    }

    // получение турнира по ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTournament(int id)
    {
        var tournament = await _db.Tournaments.FindAsync(id);

        if (tournament == null)
            return NotFound("Турнир не найден");

        await UpdateTournamentStatus(tournament);
        await _db.SaveChangesAsync();

        return Ok(tournament);
    }

    // создание турнира
    [HttpPost]
    public async Task<IActionResult> AddTournament(
        [FromBody] Tournament tournament)
    {
        if (string.IsNullOrWhiteSpace(tournament.Name))
            return BadRequest("Введите название турнира");

        if (tournament.PlayerIds == null ||
            tournament.PlayerIds.Distinct().Count() < 2)
        {
            return BadRequest("Необходимо минимум два игрока");
        }

        tournament.Id = 0;
        tournament.CreatedAt = DateTime.UtcNow;
        tournament.IsFinished = false;

        tournament.PlayerIds = tournament.PlayerIds
            .Distinct()
            .ToList();

        _db.Tournaments.Add(tournament);

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTournament),
            new { id = tournament.Id },
            tournament);
    }

    // получение или создание турнирной сетки
    [HttpGet("{id}/bracket")]
    public async Task<IActionResult> GetBracket(int id)
    {
        var tournament = await _db.Tournaments.FindAsync(id);

        if (tournament == null)
            return NotFound("Турнир не найден");

        var matches = await _db.Matches
            .Where(m => m.TournamentId == id)
            .OrderBy(m => m.Round)
            .ThenBy(m => m.Id)
            .ToListAsync();

        if (matches.Count == 0)
        {
            if (tournament.PlayerIds.Distinct().Count() < 2)
            {
                return BadRequest(
                    "Для сетки нужно минимум два игрока");
            }

            matches = _bracketService.GenerateFirstRound(
                tournament.PlayerIds);

            foreach (var match in matches)
            {
                match.TournamentId = id;
            }

            _db.Matches.AddRange(matches);
        }

        SetAutomaticWinners(matches);

        await UpdateTournamentStatus(tournament);
        await _db.SaveChangesAsync();

        return Ok(matches);
    }

    // запись победителя матча и начисление рейтинга
    [HttpPut("{id}/matches/{matchId}/winner")]
    public async Task<IActionResult> SetMatchWinner(
        int id,
        int matchId,
        [FromBody] int winnerId)
    {
        var tournament = await _db.Tournaments.FindAsync(id);

        if (tournament == null)
            return NotFound("Турнир не найден");

        var match = await _db.Matches
            .FirstOrDefaultAsync(m =>
                m.Id == matchId &&
                m.TournamentId == id);

        if (match == null)
            return NotFound("Матч не найден");

        if (match.Player1Id == null ||
            match.Player2Id == null)
        {
            return BadRequest(
                "В этом матче недостаточно участников");
        }

        if (winnerId != match.Player1Id &&
            winnerId != match.Player2Id)
        {
            return BadRequest(
                "Игрок не участвует в этом матче");
        }

        if (tournament.IsFinished && match.WinnerId == null)
            return BadRequest("Турнир уже завершён");

        if (match.WinnerId != null &&
            match.WinnerId != winnerId)
        {
            return BadRequest(
                "Победитель этого матча уже определён");
        }

        // сохраняем победителя
        if (match.WinnerId == null)
        {
            match.WinnerId = winnerId;
            await _db.SaveChangesAsync();
        }

        // начисляем рейтинг только один раз
        if (!match.RatingUpdated)
        {
            int loserId = winnerId == match.Player1Id.Value
                ? match.Player2Id.Value
                : match.Player1Id.Value;

            var ratingResult = await UpdatePlayerRatings(
                winnerId,
                loserId);

            if (!ratingResult.Success)
            {
                return StatusCode(502, new
                {
                    Message = "Результат матча сохранён, но рейтинг не обновлён. Повторите запрос.",
                    Error = ratingResult.Error
                });
            }

            match.RatingUpdated = true;

            await _db.SaveChangesAsync();
        }

        // обновляем статус турнира
        await UpdateTournamentStatus(tournament);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            Message = "Победитель сохранён, рейтинг обновлён",
            Match = match,
            IsFinished = tournament.IsFinished
        });
    }

    // создание следующего раунда
    [HttpPost("{id}/next-round")]
    public async Task<IActionResult> GenerateNextRound(int id)
    {
        var tournament = await _db.Tournaments.FindAsync(id);

        if (tournament == null)
            return NotFound("Турнир не найден");

        if (tournament.IsFinished)
            return BadRequest("Турнир уже завершён");

        var matches = await _db.Matches
            .Where(m => m.TournamentId == id)
            .ToListAsync();

        if (matches.Count == 0)
        {
            return BadRequest(
                "Турнирная сетка ещё не создана");
        }

        int currentRound = matches.Max(m => m.Round);

        var roundMatches = matches
            .Where(m => m.Round == currentRound)
            .OrderBy(m => m.Id)
            .ToList();

        // обрабатываем свободные проходы
        SetAutomaticWinners(roundMatches);

        if (roundMatches.Any(m => m.WinnerId == null))
        {
            await _db.SaveChangesAsync();

            return BadRequest(
                "Не все матчи текущего раунда завершены");
        }

        var winners = roundMatches
            .Select(m => m.WinnerId!.Value)
            .ToList();

        // проверяем наличие чемпиона
        if (winners.Count == 1)
        {
            tournament.IsFinished = true;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Message = "Турнир завершён!",
                ChampionId = winners[0]
            });
        }

        // создаём следующий раунд
        var nextMatches =
            _bracketService.GenerateFirstRound(winners);

        foreach (var match in nextMatches)
        {
            match.TournamentId = id;
            match.Round = currentRound + 1;
        }

        // определяем автоматических победителей
        SetAutomaticWinners(nextMatches);

        _db.Matches.AddRange(nextMatches);

        var allMatches = matches
            .Concat(nextMatches)
            .ToList();

        tournament.IsFinished =
            IsTournamentFinished(allMatches);

        await _db.SaveChangesAsync();

        return Ok(nextMatches);
    }
}
