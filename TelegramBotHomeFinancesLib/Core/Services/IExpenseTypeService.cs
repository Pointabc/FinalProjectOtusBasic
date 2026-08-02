using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.Services;

internal interface IExpenseTypeService
{
    /// <summary>
    /// Получить типы расходов.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Типы расходов.</returns>
    Task<IReadOnlyList<ExpenseType>> GetAllByUserId(Guid userId, CancellationToken ct);

    /// <summary>
    /// Создать тип расхода.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <param name="name">Наименование типа расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Тип расхода.</returns>
    Task<ExpenseType> Add(FinanceUser user, string name, CancellationToken ct);

    /// <summary>
    /// Удалить тип расхода.
    /// </summary>
    /// <param name="expenseTypeId">Guid типа расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Delete(Guid expenseTypeId, CancellationToken ct);

    /// <summary>
    /// Получить типы расхода.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="predicate">Условие поиска типа расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Типы расходов.</returns>
    Task<IReadOnlyList<ExpenseType>> Find(Guid userId, Func<ExpenseType, bool> predicate, CancellationToken ct);

    /// <summary>
    /// Получить тип расхода.
    /// </summary>
    /// <param name="expenseTypeId">Guid типа расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Тип расхода.</returns>
    Task<ExpenseType?> Get(Guid expenseTypeId, CancellationToken ct);
}
