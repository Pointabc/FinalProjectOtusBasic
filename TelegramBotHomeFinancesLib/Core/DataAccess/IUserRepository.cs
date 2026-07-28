using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.DataAccess
{
    internal interface IUserRepository
    {
        /// <summary>
        /// Получить пользователя по ИД.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Пользователь.</returns>
        Task<FinanceUser?> GetUser(Guid userId, CancellationToken ct);

        /// <summary>
        /// Получить пользователя по ИД в Telegtam.
        /// </summary>
        /// <param name="telegramUserId">ИД пользователя в Телеграм.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Пользователь.</returns>
        Task<FinanceUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct);

        /// <summary>
        /// Добавить пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="ct">Токен отмены.</param>
        Task Add(FinanceUser user, CancellationToken ct);
    }
}
