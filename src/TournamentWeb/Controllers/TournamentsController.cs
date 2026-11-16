
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;

namespace TournamentWeb.Controllers;

public class TournamentsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TournamentsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Список турниров
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("TournamentApi");

            var tournaments =
                await client.GetFromJsonAsync<List<TournamentDto>>(
                    "api/tournaments");

            return View("~/Views/Tournaments/Index.cshtml",
                tournaments ?? new List<TournamentDto>());
        }
        catch (HttpRequestException)
        {
            ViewBag.ApiError =
                "Не удалось подключиться к TournamentService.";

            return View("~/Views/Tournaments/Index.cshtml",
                new List<TournamentDto>());
        }
        catch (System.Text.Json.JsonException)
        {
            ViewBag.ApiError = "Ошибка обработки данных турниров.";

            return View("~/Views/Tournaments/Index.cshtml",
                new List<TournamentDto>());
        }
    }

    // Страница создания турнира
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var client = _httpClientFactory.CreateClient("PlayerApi");

        var players = await client.GetFromJsonAsync<List<PlayerDto>>(
            "api/players");

        var model = new CreateTournamentViewModel
        {
            Players = players ?? new List<PlayerDto>()
        };

        return View(model);
    }

    // Создание турнира
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTournamentViewModel model)
    {
        var selectedPlayerIds = (model.PlayerIds ?? new List<int>())
            .Distinct()
            .ToList();

        model.PlayerIds = selectedPlayerIds;

        // Проверка количества игроков
        if (selectedPlayerIds.Count < 2)
        {
            ModelState.AddModelError(
                "",
                "Выберите минимум двух игроков.");
        }

        // Если форма заполнена неправильно
        if (!ModelState.IsValid)
        {
            var playerClient =
                _httpClientFactory.CreateClient("PlayerApi");

            model.Players =
                await playerClient.GetFromJsonAsync<List<PlayerDto>>(
                    "api/players") ?? new List<PlayerDto>();

            return View(model);
        }

        // Отправляем запрос на создание турнира
        var client = _httpClientFactory.CreateClient("TournamentApi");

        var response = await client.PostAsJsonAsync(
            "api/tournaments",
            new
            {
                Name = model.Name!.Trim(),
                PlayerIds = selectedPlayerIds
            });

        // Обработка ошибки API
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                "",
                "Не удалось создать турнир: " +
                await response.Content.ReadAsStringAsync());

            var playerClient =
                _httpClientFactory.CreateClient("PlayerApi");

            model.Players =
                await playerClient.GetFromJsonAsync<List<PlayerDto>>(
                    "api/players") ?? new List<PlayerDto>();

            return View(model);
        }

        // Успешное создание
        return RedirectToAction(nameof(Index));
    }

    // Страница конкретного турнира
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var tournamentClient =
            _httpClientFactory.CreateClient("TournamentApi");

        var tournament =
            await tournamentClient.GetFromJsonAsync<TournamentDto>(
                $"api/tournaments/{id}");

        if (tournament == null)
            return NotFound("Турнир не найден");

        // Получение турнирной сетки
        var bracketResponse =
            await tournamentClient.GetAsync(
                $"api/tournaments/{id}/bracket");

        var matches = new List<MatchDto>();

        if (bracketResponse.IsSuccessStatusCode)
        {
            matches = await bracketResponse.Content
                .ReadFromJsonAsync<List<MatchDto>>()
                ?? new List<MatchDto>();
        }
        else
        {
            ViewBag.BracketError =
                await bracketResponse.Content.ReadAsStringAsync();
        }

        // Получение игроков
        var playerClient =
            _httpClientFactory.CreateClient("PlayerApi");

        var players =
            await playerClient.GetFromJsonAsync<List<PlayerDto>>(
                "api/players") ?? new List<PlayerDto>();

        // Формирование модели страницы
        var model = new TournamentDetailsViewModel
        {
            Tournament = tournament,
            Matches = matches,
            Players = players
        };

        return View(model);
    }

    // Запись победителя матча
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetWinner(
        int id,
        int matchId,
        int winnerId)
    {
        var client =
            _httpClientFactory.CreateClient("TournamentApi");

        var response = await client.PutAsJsonAsync(
            $"api/tournaments/{id}/matches/{matchId}/winner",
            winnerId);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] =
                await response.Content.ReadAsStringAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // Создание следующего раунда
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NextRound(int id)
    {
        var client =
            _httpClientFactory.CreateClient("TournamentApi");

        var response = await client.PostAsync(
            $"api/tournaments/{id}/next-round",
            null);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] =
                await response.Content.ReadAsStringAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

// Модель игрока
public class PlayerDto
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Rating { get; set; }
}

// Модель матча
public class MatchDto
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public int Round { get; set; }

    public int? Player1Id { get; set; }

    public int? Player2Id { get; set; }

    public int? WinnerId { get; set; }
}

// Модель создания турнира
public class CreateTournamentViewModel
{
    [Required(ErrorMessage = "Введите название турнира")]
    public string? Name { get; set; }

    public List<int> PlayerIds { get; set; } = new();

    public List<PlayerDto> Players { get; set; } = new();
}

// Модель страницы турнира
public class TournamentDetailsViewModel
{
    public TournamentDto Tournament { get; set; } = new();

    public List<MatchDto> Matches { get; set; } = new();

    public List<PlayerDto> Players { get; set; } = new();
}

// Модель турнира
public class TournamentDto
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public bool IsFinished { get; set; }

    public List<int> PlayerIds { get; set; } = new();
}
