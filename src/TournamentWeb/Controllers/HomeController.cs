
using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using TournamentWeb.Models;

namespace TournamentWeb.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("TournamentApi");

        try
        {
            var tournaments = await client.GetFromJsonAsync<List<TournamentDto>>(
                "api/tournaments"
            ) ?? new List<TournamentDto>();

            ViewBag.Tournaments = tournaments;

            ViewBag.TournamentCount = tournaments.Count;
            ViewBag.FinishedCount = tournaments.Count(t => t.IsFinished);
            ViewBag.ActiveCount = tournaments.Count(t => !t.IsFinished);

            ViewBag.ApiError = null;
        }
        catch (HttpRequestException)
        {
            ViewBag.Tournaments = new List<TournamentDto>();
            ViewBag.TournamentCount = 0;
            ViewBag.FinishedCount = 0;
            ViewBag.ActiveCount = 0;

            ViewBag.ApiError =
                "Не удалось подключиться к TournamentService. Проверьте, запущен ли сервис.";
        }
        catch (System.Text.Json.JsonException)
        {
            ViewBag.Tournaments = new List<TournamentDto>();
            ViewBag.TournamentCount = 0;
            ViewBag.FinishedCount = 0;
            ViewBag.ActiveCount = 0;

            ViewBag.ApiError =
                "Не удалось обработать данные турниров от TournamentService.";
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                ?? HttpContext.TraceIdentifier
        });
    }
}
