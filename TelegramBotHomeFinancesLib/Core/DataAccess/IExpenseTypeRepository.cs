using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.DataAccess;

internal interface IExpenseTypeRepository
{
    /// <summary>
    /// Получить все типы расходов.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Типы расходов.</returns>
    Task<IReadOnlyList<ExpenseType>> GetAllByUserId(Guid userId, CancellationToken ct);

    /// <summary>
    /// Получить тип расхода.
    /// </summary>
    /// <param name="expenseTypeId">ИД типа расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Тип расхода.</returns>
    Task<ExpenseType?> Get(Guid expenseTypeId, CancellationToken ct);

    /// <summary>
    /// Добавить тип расхода.
    /// </summary>
    /// <param name="expenseType">Тип расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Add(ExpenseType expenseType, CancellationToken ct);

    /// <summary>
    /// Удалить тип расхода.
    /// </summary>
    /// <param name="expenseTypeId">ИД типа расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Delete(Guid expenseTypeId, CancellationToken ct);

    /// <summary>
    /// Найти типы расходов.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="predicate">Условие поиска типа расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Типы расходов.</returns>
    Task<IReadOnlyList<ExpenseType>> Find(Guid userId, Func<ExpenseType, bool> predicate, CancellationToken ct);
}
