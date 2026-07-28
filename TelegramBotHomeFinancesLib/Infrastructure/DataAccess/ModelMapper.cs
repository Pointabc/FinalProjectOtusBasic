using TelegramBotHomeFinancesLib.Core.Entities;
using TelegramBotHomeFinancesLib.Core.DataAccess.Models;

namespace TelegramBotHomeFinancesLib.Infrastructure.DataAccess;

internal static class ModelMapper
{
    /// <summary>
    /// Преобразовать модель БД пользователя в пользователя.
    /// </summary>
    /// <param name="model">Модель БД пользователя.</param>
    /// <returns>Пользователь.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static FinanceUser MapFromModel(FinanceUserModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new FinanceUser
        {
            TelegramUserId = model.TelegramUserId,
            FinanceUserId = model.FinanceUserId,
            TelegramUserName = model.TelegramUserName,
            RegisteredAt = model.RegisteredAt
        };
    }

    /// <summary>
    /// Преобразовать пользователя в модель БД пользователя.
    /// </summary>
    /// <param name="entity">Пользователь.</param>
    /// <returns>Модель БД пользователя.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static FinanceUserModel MapToModel(FinanceUser entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new FinanceUserModel
        {
            TelegramUserId = entity.TelegramUserId,
            FinanceUserId = entity.FinanceUserId,
            TelegramUserName = entity.TelegramUserName,
            RegisteredAt = entity.RegisteredAt,
        };
    }

    /// <summary>
    /// Преобразовать модель БД типа расхода в тип расхода.
    /// </summary>
    /// <param name="model">Модель БД типа расхода.</param>
    /// <returns>Тип расхода.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExpenseType MapFromModel(ExpenseTypeModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new ExpenseType
        {
            Name = model.Name,
            CreatedAt = model.CreatedAt,
        };
    }

    /// <summary>
    /// Преобразовать тип расхода в модель БД типа расхода.
    /// </summary>
    /// <param name="entity">Тип расхода.</param>
    /// <returns>Модель БД типа расхода.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExpenseTypeModel MapToModel(ExpenseType entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new ExpenseTypeModel
        {
            Name = entity.Name,
            CreatedAt = entity.CreatedAt,
        };
    }

    /// <summary>
    /// Преобразовать модель БД расхода в расход.
    /// </summary>
    /// <param name="model">Модель БД расхода.</param>
    /// <returns>Расход.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Expense MapFromModel(ExpenseModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new Expense
        {
            ExpenseId = model.ExpenseId,
            expenseType = model.ExpenseType,
            Amount = model.Amount,
            Note = model.Note,
            User = model.User,
            CreatedAt = model.CreatedAt,
        };
    }

    /// <summary>
    /// Преобразовать расход в модель БД расхода.
    /// </summary>
    /// <param name="entity">Расход.</param>
    /// <returns>Модель БД расхода.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExpenseModel MapToModel(Expense entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new ExpenseModel
        {
            ExpenseId = entity.ExpenseId,
            ExpenseTypeId = entity.expenseType.ExpenseTypeId,
            ExpenseType = entity.expenseType,
            Amount = entity.Amount,
            Note = entity.Note,
            FinanceUserId = entity.User.FinanceUserId,
            CreatedAt = entity.CreatedAt,
        };
    }

    /// <summary>
    /// Преобразовать модель БД типа прихода в тип прихода.
    /// </summary>
    /// <param name="model">Модель БД типа прихода.</param>
    /// <returns>Тип прихода.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IncomeType MapFromModel(IncomeTypeModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new IncomeType
        {
            Name = model.Name,
            IncomeTypeId = model.IncomeTypeId,
            User = model.User,
            CreatedAt = model.CreatedAt,
        };
    }

    /// <summary>
    /// Преобразовать тип прихода в модель БД типа прихода.
    /// </summary>
    /// <param name="entity">Тип прихода.</param>
    /// <returns>Модель БД типа прихода.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IncomeTypeModel MapToModel(IncomeType entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new IncomeTypeModel
        {
            Name = entity.Name,
            FinanceUserId = entity.User.FinanceUserId,
            IncomeTypeId = entity.IncomeTypeId,
            User = entity.User,
            CreatedAt = entity.CreatedAt,
        };
    }

    /// <summary>
    /// Преобразовать модель БД прихода в приход.
    /// </summary>
    /// <param name="model">Модель БД прихода.</param>
    /// <returns>Приход.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Income MapFromModel(IncomeModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new Income
        {
            IncomeId = model.IncomeId,
            IncomeType = model.IncomeType,
            Amount = model.Amount,
            Note = model.Note,
            User = model.User,
            CreatedAt = model.CreatedAt,
        };
    }

    /// <summary>
    /// Преобразовать приход в модель БД прихода.
    /// </summary>
    /// <param name="entity">Приход.</param>
    /// <returns>Модель БД прихода.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IncomeModel MapToModel(Income entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new IncomeModel
        {
            IncomeId = entity.IncomeId,
            IncomeTypeId = entity.IncomeType.IncomeTypeId,
            IncomeType = entity.IncomeType,
            Amount = entity.Amount,
            Note = entity.Note,
            FinanceUserId = entity.User.FinanceUserId,
            CreatedAt = entity.CreatedAt,
        };
    }
}
