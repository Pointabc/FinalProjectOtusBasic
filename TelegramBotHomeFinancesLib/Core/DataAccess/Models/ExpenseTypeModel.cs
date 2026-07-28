using LinqToDB.Mapping;

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
    public DateTime CreatedAt { get; set; }
}
