namespace TelegramBotHomeFinancesLib.DTO;

internal class IncomeCallbackDto : CallbackDto
{
    public Guid? IncomeId { get; set; }

    /// <summary>
    /// Создать объект IncomeCallbackDto.
    /// </summary>
    /// <param name="input">Строка ввида "{action}|{IncomeId}|{prop2}.</param>
    /// <returns>Объект IncomeCallbackDto.</returns>
    public static new IncomeCallbackDto FromString(string input)
    {
        var splitInput = input.Split('|');
        var incomeCallbackDto = new IncomeCallbackDto();
        incomeCallbackDto.Action = splitInput.Length == 1 ? input : splitInput[0];
        incomeCallbackDto.IncomeId = splitInput.Length > 1 && splitInput[1] != string.Empty ? Guid.Parse(splitInput[1]) : null;

        return incomeCallbackDto;
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{IncomeId}";
    }
}
