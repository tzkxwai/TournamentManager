
using Microsoft.EntityFrameworkCore;
using TournamentService.Data;
using TournamentService.Services;

var builder = WebApplication.CreateBuilder(args);

// Подключение генератора турнирной сетки
builder.Services.AddScoped<TournamentBracketService>();

// Подключение базы данных турниров
builder.Services.AddDbContext<TournamentDbContext>(options =>
    options.UseSqlite("Data Source=tournaments.db"));

// Подключение PlayerService для ELO
builder.Services.AddHttpClient("PlayerApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5244/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Подключение контроллеров
builder.Services.AddControllers();

// Подключение OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
