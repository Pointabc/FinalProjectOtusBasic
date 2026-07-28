namespace TelegramBotHomeFinancesLib.Core.Entities;

public class ExpenseType
{
    public Guid ExpenseTypeId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}
