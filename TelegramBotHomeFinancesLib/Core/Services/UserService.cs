using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.Services;

internal class UserService : IUserService
{
    IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Получить пользователя.
    /// </summary>
    /// <param name="telegramUserId">ИД пользователя в Telegram.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пользователь.</returns>
    public async Task<FinanceUser?> GetUser(long telegramUserId, CancellationToken cancellationToken)
    {
        return await _userRepository.GetUserByTelegramUserId(telegramUserId, cancellationToken);
    }

    /// <summary>
    /// Зарегистрировать пользователя.
    /// </summary>
    /// <param name="telegramUserId">ИД пользователя в Telegram.</param>
    /// <param name="telegramUserName">Имя пользователя в Telegram.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пользователь.</returns>
    public async Task<FinanceUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
    {
        var user = new FinanceUser
        {
            TelegramUserName = telegramUserName,
            TelegramUserId = telegramUserId,
            FinanceUserId = Guid.NewGuid(),
            RegisteredAt = DateTime.UtcNow,
        };

        await _userRepository.Add(user, cancellationToken);

        return user;
    }
}
