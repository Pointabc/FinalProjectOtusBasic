namespace TelegramBotHomeFinancesLib.Core.Entities;

public class IncomeType
{
    public Guid IncomeTypeId { get; set; }
    public string Name { get; set; }
    public FinanceUser User { get; set; }
    public DateTime CreatedAt { get; set; }
}
