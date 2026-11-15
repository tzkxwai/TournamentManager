## 2026-11-16 — Разработка TournamentService

Начал разработку сервиса для проведения турниров.

Используются .NET 10, ASP.NET Core Web API, Entity Framework Core и SQLite.

### Создание турниров

Реализовал:
- получение списка турниров;
- получение турнира по ID;
- создание нового турнира;
- сохранение информации о турнирах в SQLite.

Для проверки API использовал PowerShell и Invoke-RestMethod.

### Создание турнирной сетки

Создал класс `TournamentBracketService`.

Реализовал генерацию олимпийской турнирной сетки:
- удаление повторяющихся ID игроков;
- проверка минимального количества участников;
- создание пар первого раунда;
- распределение участников по матчам.

Добавил сохранение матчей в таблицу `Matches`.

### Результаты матчей

В `TournamentsController` добавил возможность:
- записывать победителя матча;
- проверять участие игрока в матче;
- получать сохранённую турнирную сетку;
- создавать следующий раунд;
- определять завершение турнира.

После завершения финального матча турнир получает статус завершённого.

Также добавил защиту от изменения результатов завершённого турнира.

### Миграции базы данных

Добавил модель `TournamentMatch` и таблицу `Matches`.

Создал миграцию `AddTournamentMatches` и применил её к SQLite.

Использованные команды:

```powershell
dotnet ef migrations add AddTournamentMatches --project src/TournamentService --startup-project src/TournamentService
```

```powershell
dotnet ef database update --project src/TournamentService --startup-project src/TournamentService
```

### Автоматические тесты TournamentService

Создал проект `TournamentService.Tests` на .NET 10 с xUnit.

Добавил ссылку на `TournamentService` и пакет `Microsoft.EntityFrameworkCore.Sqlite` версии `10.0.12`.

Создал два файла с тестами.

`TournamentBracketTests.cs`:
- `FourPlayers_ShouldCreateTwoMatches` — проверка сетки для четырёх игроков;
- `TwoPlayers_ShouldCreateOneMatch` — проверка сетки для двух игроков;
- `DuplicatePlayers_ShouldBeRemoved` — проверка удаления повторяющихся участников;
- `OnePlayer_ShouldThrowException` — проверка запрета турнира с одним игроком.

`TournamentDatabaseTests.cs`:
- `AddTournament_ShouldSaveToDatabase` — сохранение турнира;
- `AddMatch_ShouldSaveToDatabase` — сохранение матча;
- `UpdateWinner_ShouldSaveToDatabase` — сохранение победителя.

Для тестов базы данных используется SQLite в памяти (`Data Source=:memory:`). Основной файл `tournaments.db` при тестировании не изменяется.

### Результаты тестирования

Сначала отдельно проверил `TournamentService.Tests`:

- всего тестов: 7;
- успешно: 7;
- ошибок: 0;
- пропущено: 0.

Затем запустил тестирование всего решения:

```powershell
dotnet test TournamentManager.slnx
```

Итоговый результат:

| Проект | Результат |
|---|---:|
| PlayerService.Tests | 6 тестов успешно |
| TournamentService.Tests | 7 тестов успешно |
| Всего | 13 тестов успешно |

Ошибок: 0.

Сборка всех четырёх проектов прошла успешно.

## Текущее состояние проекта на 16.11.2026

Реализованы два основных сервиса:
- `PlayerService`;
- `TournamentService`.

Созданы два тестовых проекта:
- `PlayerService.Tests`;
- `TournamentService.Tests`.

Реализованы основные функции создания турниров, генерации сетки, сохранения матчей, записи победителей и формирования следующих раундов.

Созданы и обновлены документы:
- `ARCHITECTURE.md`;
- `DEVELOPMENT_LOG.md`;
- `PLAN.md`.

Общее количество успешно пройденных автоматических тестов — 13.

## Что дальше

Следующие задачи разработки:

1. Проверить состояние Git и сохранить последние изменения в репозитории.
2. Подготовить веб-интерфейс для управления турнирами.
3. Реализовать систему рейтинга Эло.
4. Добавить авторизацию и роли пользователей.
5. Рассмотреть выделение MatchService в отдельный сервис.
6. Подготовить демонстрацию проекта для защиты учебной практики.
