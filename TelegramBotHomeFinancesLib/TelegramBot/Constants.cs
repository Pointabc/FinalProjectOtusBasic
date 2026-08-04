namespace TelegramBotHomeFinancesLib.TelegramBot;

internal class Constants
{
    #region Команды бота.

    public const string CommandStart = "/start";
    public const string CommandHelp = "/help";
    public const string CommandInfo = "/info";

    // Функции роли вносящего данные.
    public const string CommandAddIncome = "/addincome"; // Добавить доход.
    public const string CommandAddExpense = "/addexpense"; // Добавить расход.
    public const string CommandViewBalance = "/viewbalance"; // Посмотреть баланс.
    public const string CommandSendBalance = "/sendbalance"; // Отправить баланс.
    public const string CommandShowTypeIncome = "/showtypeincome"; // Получить виды доходов.
    public const string CommandShowTypeExpense = "/showtypeexpense"; // Получить виды расходов.
    public const string CommandCancel = "/cancel";

    #endregion

    /// <summary>
    /// Дата создания бота (начало разработки бота).
    /// </summary>
    public static readonly DateTime CreatedDate = new DateTime(2026, 3, 15);

    public const string UnknownCommand = "Неизвестная команда.";

    #region Ключи для Dictionary

    public const string KeyUserIdName = "userId";

    #endregion

    #region Названия действий (Action) для CallbackDto

    public const string ActionNameShow = "show";
    public const string ActionNameAddIncomeType = "addincometype";
    public const string ActionNameDeleteIncomeType = "deleteincometype";
    public const string ActionNameAddExpenseType = "addexpensetype";
    public const string ActionNameDeleteExpenseType = "deleteexpensetype";

    #endregion
}
