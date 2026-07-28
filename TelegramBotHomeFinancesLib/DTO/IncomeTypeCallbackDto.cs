namespace TelegramBotHomeFinancesLib.DTO;

internal class IncomeTypeCallbackDto : CallbackDto
{
    public Guid? IncomeTypeId { get; set; }

    /// <summary>
    /// Создать объект IncomeTypeCallbackDto.
    /// </summary>
    /// <param name="input">Строка ввида "{action}|{IncomeTypeId}|{prop2}.</param>
    /// <returns>Объект IncomeTypeCallbackDto.</returns>
    public static new IncomeTypeCallbackDto FromString(string input)
    {
        var splitInput = input.Split('|');
        var incomeTypeCallbackDto = new IncomeTypeCallbackDto();
        incomeTypeCallbackDto.Action = splitInput.Length == 1 ? input : splitInput[0];
        incomeTypeCallbackDto.IncomeTypeId = splitInput.Length > 1 && splitInput[1] != string.Empty ? Guid.Parse(splitInput[1]) : null;

        return incomeTypeCallbackDto;
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{IncomeTypeId}";
    }
}
