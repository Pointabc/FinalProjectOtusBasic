using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotHomeFinancesLib.Core.Services;
using TelegramBotHomeFinancesLib.TelegramBot;

namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal class AddIncomeTypeScenario : IScenario
{
    IUserService _userService;
    IIncomeTypeService _incomeTypeService;

    public AddIncomeTypeScenario(IUserService userService, IIncomeTypeService incomeTypeService)
    {
        _userService = userService ?? throw new ArgumentNullException();
        _incomeTypeService = incomeTypeService ?? throw new ArgumentNullException();
    }

    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.AddIncomeType;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
    {
        var scenarioResult = ScenarioResult.Transition;
        var telegramUser = UpdateHandler.GetUserFromUpdate(update);
        var financeUser = await _userService.GetUser(telegramUser.Id, ct);
        if (financeUser == null)
            return scenarioResult;

        var chat = UpdateHandler.GetChatFromUpdate(update);
        var currentStep = context.CurrentStep;
        ReplyKeyboardMarkup _replyKeyboard = await UpdateHandler.CreateKeyboardMarkupCancel();
        ReplyKeyboardMarkup _replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
        var userInput = UpdateHandler.GetMessageFromUpdate(update);

        switch (currentStep)
        {
            case null:
                context.Data.Add(financeUser.TelegramUserId.ToString(), financeUser);
                await bot.SendMessage(chat, "Введите название типа прихода:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                context.CurrentStep = "Name";
                break;
            case "Name":
                try
                {
                    // Получить пользователя из контекста.
                    var incomeType = await _incomeTypeService.Add(financeUser, userInput, ct);
                    if (incomeType == null)
                    {
                        await bot.SendMessage(
                            chat,
                            $"Тип прихода не создан. Сообщение: {userInput}.)",
                            cancellationToken: ct);
                        break;
                    }

                    context.CurrentStep = "Тип прихода создан.";
                    scenarioResult = ScenarioResult.Completed;
                    await bot.SendMessage(chat, "Тип прихода добавлен.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await bot.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
                    /*switch (currentStep)
                    {
                        case "Name":
                            await bot.SendMessage(chat, "Введите название списка:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                            break;
                    }*/
                }
                break;
            case "Cancel":
                scenarioResult = ScenarioResult.Completed;
                context.CurrentStep = "Сценарий завершен.";
                await bot.SendMessage(chat, "Операция отменена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                break;
            default:
                break;
        }

        return scenarioResult;
    }
}
