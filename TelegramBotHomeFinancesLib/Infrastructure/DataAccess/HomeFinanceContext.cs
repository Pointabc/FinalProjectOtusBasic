using LinqToDB;
using LinqToDB.Data;
using TelegramBotHomeFinancesLib.Core.DataAccess.Models;

namespace TelegramBotHomeFinancesLib.Infrastructure.DataAccess
{
    internal class HomeFinanceContext : DataConnection
    {
        public HomeFinanceContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString) { }

        public ITable<IncomeTypeModel> IncomeTypes => this.GetTable<IncomeTypeModel>();
        public ITable<ExpenseTypeModel> ExpenseTypes => this.GetTable<ExpenseTypeModel>();
        public ITable<IncomeModel> Incomes => this.GetTable<IncomeModel>();
        public ITable<ExpenseModel> Expenses => this.GetTable<ExpenseModel>();
        public ITable<FinanceUserModel> FinanceUsers => this.GetTable<FinanceUserModel>();
    }
}
