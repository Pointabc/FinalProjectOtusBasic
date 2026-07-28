using LinqToDB.Mapping;

namespace TelegramBotHomeFinancesLib.Core.DataAccess.Models;

[Table("FinanceUser")]
public class FinanceUserModel
{
    [PrimaryKey]
    public Guid FinanceUserId { get; set; }
    [Column]
    public long TelegramUserId { get; set; }
    [Column]
    public string TelegramUserName { get; set; }
    [Column]
    public DateTime RegisteredAt { get; set; }
}
