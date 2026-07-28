using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.DataAccess;

internal interface IIncomeRepository
{
    /// <summary>
    /// Получить все приходы пользователя.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Приходы.</returns>
    Task<IReadOnlyList<Income>> GetAllByUserId(Guid userId, CancellationToken ct);

    /// <summary>
    /// Получить приход.
    /// </summary>
    /// <param name="incomeId">ИД расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Приход.</returns>
    Task<Income?> Get(Guid incomeId, CancellationToken ct);

    /// <summary>
    /// Добавить приход.
    /// </summary>
    /// <param name="income">Приход.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Add(Income income, CancellationToken ct);

    /// <summary>
    /// Удалить приход.
    /// </summary>
    /// <param name="incomeId">ИД прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Delete(Guid incomeId, CancellationToken ct);

    /// <summary>
    /// Найти приходы.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="predicate">Условие поиска прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Приход.</returns>
    Task<IReadOnlyList<Income>> Find(Guid userId, Func<Income, bool> predicate, CancellationToken ct);
}
