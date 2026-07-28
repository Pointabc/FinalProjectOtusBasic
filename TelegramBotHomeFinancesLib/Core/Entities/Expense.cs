namespace TelegramBotHomeFinancesLib.Core.Entities;

public class Expense
{
    public Guid ExpenseId { get; set; }
    public ExpenseType expenseType { get; set; }
    public Decimal Amount { get; set; }
    public string? Note { get; set; }
    public FinanceUser User { get; set; }
    public DateTime CreatedAt { get; set; }
}
