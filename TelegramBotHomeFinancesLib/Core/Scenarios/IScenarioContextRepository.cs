namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal interface IScenarioContextRepository
{
    /// <summary>
    /// Получить контекст пользователя.
    /// </summary>
    /// <param name="userId">Id пользователя в Telegram.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Сессия/контекст пользователя.</returns>
    Task<ScenarioContext?> GetContext(long userId, CancellationToken ct);

    /// <summary>
    /// Задать сессию/контекст пользователя.
    /// </summary>
    /// <param name="userId">Id пользователя в Telegram.</param>
    /// <param name="context">Сессия/контекст.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Сессия/контекст пользователя.</returns>
    Task SetContext(long userId, ScenarioContext context, CancellationToken ct);

    /// <summary>
    /// Сбросить (очистить) контекст пользователя.
    /// </summary>
    /// <param name="userId">Id пользователя в Telegram.</param>
    /// <param name="ct">Токен отмены.</param>
    Task ResetContext(long userId, CancellationToken ct);

    /// <summary>
    /// Получить все контексты.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Все контексты.</returns>
    Task<IReadOnlyList<ScenarioContext>> GetContexts(CancellationToken ct);
}
