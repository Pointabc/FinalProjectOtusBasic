using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.DataAccess;

internal interface IExpenseRepository
{
    /// <summary>
    /// Получить все расходы пользователя.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Расходы.</returns>
    Task<IReadOnlyList<Expense>> GetAllByUserId(Guid userId, CancellationToken ct);

    /// <summary>
    /// Получить расход.
    /// </summary>
    /// <param name="expenseId">ИД расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Расход.</returns>
    Task<Expense?> Get(Guid expenseId, CancellationToken ct);

    /// <summary>
    /// Добавить расход.
    /// </summary>
    /// <param name="expense">Расход.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Add(Expense expense, CancellationToken ct);

    /// <summary>
    /// Удалить расход.
    /// </summary>
    /// <param name="expenseId">ИД расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Delete(Guid expenseId, CancellationToken ct);

    /// <summary>
    /// Найти расходы.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="predicate">Условие поиска расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Расход.</returns>
    Task<IReadOnlyList<Expense>> Find(Guid userId, Func<Expense, bool> predicate, CancellationToken ct);
}
