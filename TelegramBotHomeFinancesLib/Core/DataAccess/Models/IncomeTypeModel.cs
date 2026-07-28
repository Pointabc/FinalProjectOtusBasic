using LinqToDB.Mapping;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.DataAccess.Models;

/// <summary>
/// Тип прихода.
/// </summary>
[Table("IncomeType")]
public class IncomeTypeModel
{
    [PrimaryKey]
    public Guid IncomeTypeId { get; set; }
    [Column]
    public string Name { get; set; }
    [Column]
    public Guid FinanceUserId { get; set; }
    [Association(ThisKey = nameof(FinanceUserId), OtherKey = nameof(FinanceUserModel.FinanceUserId))]
    public FinanceUser User { get; set; }
    [Column]
    public DateTime CreatedAt { get; set; }
}
