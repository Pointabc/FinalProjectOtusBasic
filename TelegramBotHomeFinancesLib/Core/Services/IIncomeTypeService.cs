using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.Services;

internal interface IIncomeTypeService
{
    /// <summary>
    /// Получить типы приходов.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Типы приходов.</returns>
    Task<IReadOnlyList<IncomeType>> GetAllByUserId(Guid userId, CancellationToken ct);

    /// <summary>
    /// Создать тип прихода.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <param name="name">Наименование типа прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Тип прихода.</returns>
    Task<IncomeType> Add(FinanceUser user, string name, CancellationToken ct);

    /// <summary>
    /// Удалить тип прихода.
    /// </summary>
    /// <param name="incomeTypeId">Guid типа прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    Task Delete(Guid incomeTypeId, CancellationToken ct);

    /// <summary>
    /// Получить типы прихода.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="predicate">Условие поиска типа прихода.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Типа приходов.</returns>
    Task<IReadOnlyList<IncomeType>> Find(Guid userId, Func<IncomeType, bool> predicate, CancellationToken ct);

    /// <summary>
    /// Получить тип прихода.
    /// </summary>
    /// <param name="incomeTypeId">Guid типа прихода.</param>
    /// <param name="ct"></param>
    /// <returns>Тип прихода.</returns>
    Task<IncomeType?> Get(Guid incomeTypeId, CancellationToken ct);
}
