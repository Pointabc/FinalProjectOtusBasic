namespace TelegramBotHomeFinancesLib.DTO;

internal class CallbackDto
{
    /// <summary>
    /// Определять за какое действие отвечает кнопка Методы.
    /// </summary>
    public string Action { get; set; }

    public static CallbackDto FromString(string input)
    {
        var splitInput = input.Split('|');
        return new CallbackDto { Action = splitInput.FirstOrDefault() };
    }

    public override string ToString()
    {
        return Action;
    }
}
