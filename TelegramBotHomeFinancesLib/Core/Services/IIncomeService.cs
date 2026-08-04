using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.Services;

internal interface IIncomeService
{
    /// <summary>
    /// Получить приходы.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Приходы.</returns>
    Task<IReadOnlyList<Income>> GetAllByUserId(Guid userId, CancellationToken ct);

    /// <summary>
    /// Создать приход.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <param name="incomeType">Тип прихода.</param>
    /// <param name="amount">Сумма.</param>
    /// <param name="note">Примечание.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Приход.</returns>
    Task<Income> Add(FinanceUser user, IncomeType? incomeType, Decimal amount, string note, CancellationToken ct);

    /// <summary>
    /// Удалить приход.
    /// </summary>
    /// <param name="incomeId">Guid прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Delete(Guid incomeId, CancellationToken ct);

    /// <summary>
    /// Получить приходы.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="predicate">Условие поиска прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Приходы.</returns>
    Task<IReadOnlyList<Income>> Find(Guid userId, Func<Income, bool> predicate, CancellationToken ct);

    /// <summary>
    /// Получить приход.
    /// </summary>
    /// <param name="incomeId">Guid прихода.</param>
    /// <param name="ct"></param>
    /// <returns>Приход.</returns>
    Task<Income?> Get(Guid incomeId, CancellationToken ct);
}
