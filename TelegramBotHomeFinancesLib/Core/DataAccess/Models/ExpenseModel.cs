using LinqToDB.Mapping;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.DataAccess.Models
{
    /// <summary>
    /// Расход.
    /// </summary>
    [Table("Expense")]
    public class ExpenseModel
    {
        [PrimaryKey]
        public Guid ExpenseId { get; set; }
        [Column]
        public Guid? ExpenseTypeId { get; set; }
        [Association(ThisKey = nameof(ExpenseTypeId), OtherKey = nameof(ExpenseTypeModel.ExpenseTypeId))]
        public ExpenseType? ExpenseType { get; set; }
        [Column]
        public decimal Amount { get; set; }
        [Column]
        public string Note { get; set; }
        [Column]
        public Guid FinanceUserId { get; set; }
        [Association(ThisKey = nameof(FinanceUserId), OtherKey = nameof(FinanceUserModel.FinanceUserId))]
        public FinanceUser User { get; set; }
        [Column]
        public DateTime CreatedAt { get; set; }
    }
}
