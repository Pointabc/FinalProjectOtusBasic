using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.Services;

internal interface IExpenseService
{
    /// <summary>
    /// Получить расходы.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Расходы.</returns>
    Task<IReadOnlyList<Expense>> GetAllByUserId(Guid userId, CancellationToken ct);

    /// <summary>
    /// Создать расход.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <param name="expenseType">Тип расхода.</param>
    /// <param name="amount">Сумма.</param>
    /// <param name="note">Примечание.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Расход.</returns>
    Task<Expense> Add(FinanceUser user, ExpenseType? expenseType, Decimal amount, string note, CancellationToken ct);

    /// <summary>
    /// Удалить расход.
    /// </summary>
    /// <param name="expenseId">Guid расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Delete(Guid expenseId, CancellationToken ct);

    /// <summary>
    /// Получить расходы.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="predicate">Условие поиска расхода.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Расходы.</returns>
    Task<IReadOnlyList<Expense>> Find(Guid userId, Func<Expense, bool> predicate, CancellationToken ct);

    /// <summary>
    /// Получить расход.
    /// </summary>
    /// <param name="expenseId">Guid расхода.</param>
    /// <param name="ct"></param>
    /// <returns>Расход.</returns>
    Task<Expense?> Get(Guid expenseId, CancellationToken ct);
}
