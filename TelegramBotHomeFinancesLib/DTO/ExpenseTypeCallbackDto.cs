namespace TelegramBotHomeFinancesLib.DTO;

internal class ExpenseTypeCallbackDto : CallbackDto
{
    public Guid? ExpenseTypeId { get; set; }

    /// <summary>
    /// Создать объект ExpenseTypeCallbackDto.
    /// </summary>
    /// <param name="input">Строка ввида "{action}|{ExpenseTypeId}|{prop2}.</param>
    /// <returns>Объект ExpenseTypeCallbackDto.</returns>
    public static new ExpenseTypeCallbackDto FromString(string input)
    {
        var splitInput = input.Split('|');
        var expenseTypeCallbackDto = new ExpenseTypeCallbackDto();
        expenseTypeCallbackDto.Action = splitInput.Length == 1 ? input : splitInput[0];
        expenseTypeCallbackDto.ExpenseTypeId = splitInput.Length > 1 && splitInput[1] != string.Empty ? Guid.Parse(splitInput[1]) : null;

        return expenseTypeCallbackDto;
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{ExpenseTypeId}";
    }
}
