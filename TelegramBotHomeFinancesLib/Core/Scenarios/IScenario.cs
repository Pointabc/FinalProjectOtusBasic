using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal interface IScenario
{
    /// <summary>
    /// Проверяет, может ли текущий сценарий обрабатывать указанный тип сценария. Используется для определения подходящего обработчика в системе сценариев.
    /// </summary>
    /// <param name="scenario">Сессия/сценарий.</param>
    /// <returns>True - может, иначе нет.</returns>
    bool CanHandle(ScenarioType scenarioType);

    /// <summary>
    /// Обрабатывает входящее сообщение от пользователя в рамках текущего сценария. Включает основную бизнес-логику.
    /// </summary>
    /// <param name="bot">Телеграм бот.</param>
    /// <param name="context">Сессия/сценарий.</param>
    /// <param name="update">Входящее обновление.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Результат сессии/сценария.</returns>
    Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct);
}
