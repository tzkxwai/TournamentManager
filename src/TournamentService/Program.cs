
using Microsoft.EntityFrameworkCore;
using TournamentService.Data;
using TournamentService.Services;

var builder = WebApplication.CreateBuilder(args);

// подключаем генератор турнирной сетки
builder.Services.AddScoped<TournamentBracketService>();

// подключаем базу данных турниров
builder.Services.AddDbContext<TournamentDbContext>(options =>
    options.UseSqlite("Data Source=tournaments.db"));

// подключаем контроллеры
builder.Services.AddControllers();

// подключаем OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// включаем OpenAPI в режиме разработки
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
