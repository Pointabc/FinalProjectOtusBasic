namespace TelegramBotHomeFinancesLib.Core.Entities;

public class Income
{
    public Guid IncomeId { get; set; }
    public Guid IncomeTypeId { get; set; }
    public IncomeType IncomeType { get; set; }
    public decimal Amount { get; set; }
    public string Note { get; set; }
    public FinanceUser User { get; set; }
    public DateTime CreatedAt { get; set; }
}
