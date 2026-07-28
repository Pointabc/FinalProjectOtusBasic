using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.DataAccess;

internal interface IIncomeTypeRepository
{
    /// <summary>
    /// Получить все типы приходов.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Типы приходов.</returns>
    Task<IReadOnlyList<IncomeType>> GetAllByUserId(Guid userId, CancellationToken ct);

    /// <summary>
    /// Получить тип прихода.
    /// </summary>
    /// <param name="incomeId">ИД расхода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Тип прихода.</returns>
    Task<IncomeType?> Get(Guid incomeTypeId, CancellationToken ct);

    /// <summary>
    /// Добавить тип прихода.
    /// </summary>
    /// <param name="income">Тип прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Add(IncomeType incomeType, CancellationToken ct);

    /// <summary>
    /// Удалить тип прихода.
    /// </summary>
    /// <param name="incomeId">ИД типа прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Delete(Guid incomeTypeId, CancellationToken ct);

    /// <summary>
    /// Найти типы приходов.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="predicate">Условие поиска типа прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Типы приходов.</returns>
    Task<IReadOnlyList<IncomeType>> Find(Guid userId, Func<IncomeType, bool> predicate, CancellationToken ct);
}
