using TelegramBotHomeFinancesLib.Core.Entities;
namespace TelegramBotHomeFinancesLib.Core.Services
{
    internal interface IUserService
    {
        /// <summary>
        /// Зарегистрировать пользователя.
        /// </summary>
        /// <param name="telegramUserId">ИД пользователя Telegram.</param>
        /// <param name="telegramUserName">Имя пользователя Telegram.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns></returns>
        Task<FinanceUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken);

        /// <summary>
        /// Получить пользователя (зарегистрированного).
        /// </summary>
        /// <param name="telegramUserId">ИД пользователя Telegram.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Зарегистрированный пользователь, иначе null.</returns>
        Task<FinanceUser?> GetUser(long telegramUserId, CancellationToken cancellationToken);
    }
}
