using LinqToDB.Mapping;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.DataAccess.Models;

/// <summary>
/// Тип расхода.
/// </summary>
[Table("ExpenseType")]
public class ExpenseTypeModel
{
    [PrimaryKey]
    public Guid ExpenseTypeId { get; set; }
    [Column]
    public string Name { get; set; }
    [Column]
    public Guid FinanceUserId { get; set; }
    [Association(ThisKey = nameof(FinanceUserId), OtherKey = nameof(FinanceUserModel.FinanceUserId))]
    public FinanceUser User { get; set; }
    [Column]
    public DateTime CreatedAt { get; set; }
}
