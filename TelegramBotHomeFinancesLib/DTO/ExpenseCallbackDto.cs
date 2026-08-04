namespace TelegramBotHomeFinancesLib.DTO;

internal class ExpenseCallbackDto : CallbackDto
{
    public Guid? ExpenseId { get; set; }

    /// <summary>
    /// Создать объект ExpenseCallbackDto.
    /// </summary>
    /// <param name="input">Строка ввида "{action}|{IncomeId}|{prop2}.</param>
    /// <returns>Объект ExpenseCallbackDto.</returns>
    public static new ExpenseCallbackDto FromString(string input)
    {
        var splitInput = input.Split('|');
        return new ExpenseCallbackDto
        {
            Action = splitInput.Length == 1 ? input : splitInput[0],
            ExpenseId = splitInput.Length > 1 && splitInput[1] != string.Empty ? Guid.Parse(splitInput[1]) : null
        };
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{ExpenseId}";
    }
}
