using LinqToDB.Mapping;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.DataAccess.Models
{
    /// <summary>
    /// Приход.
    /// </summary>
    [Table("Income")]
    public class IncomeModel
    {
        [PrimaryKey]
        public Guid IncomeId { get; set; }
        [Column]
        public Guid IncomeTypeId { get; set; }
        [Association(ThisKey = nameof(IncomeTypeId), OtherKey = nameof(IncomeTypeModel.IncomeTypeId))]
        public IncomeType IncomeType { get; set; }
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
