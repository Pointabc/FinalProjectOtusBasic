namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal enum ScenarioResult
{
    Transition, // Переход к следующему шагу. Сообщение обработано, но сценарий еще не завершен.
    Completed   // Сценарий завершен.
}
