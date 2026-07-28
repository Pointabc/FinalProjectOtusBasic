
namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal class ScenarioContext
{
    public ScenarioContext(ScenarioType scenarioType)
    {
        CurrentScenario = scenarioType;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Id пользователя в Telegram.
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// Тип сессиии/сценария.
    /// </summary>
    public ScenarioType CurrentScenario { get; set; }
    /// <summary>
    /// Текущий шаг сценария.
    /// </summary>
    public string? CurrentStep { get; set; }

    /// <summary>
    /// Дополнительная информация, необходимая для работы сценария.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; }
}
