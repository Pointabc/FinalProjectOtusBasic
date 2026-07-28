namespace TelegramBotHomeFinancesLib.Core.Entities;

public class FinanceUser
{
    public long TelegramUserId { get; set; }
    public Guid FinanceUserId { get; set; }
    public string TelegramUserName { get; set; }
    public DateTime RegisteredAt { get; set; }
}
