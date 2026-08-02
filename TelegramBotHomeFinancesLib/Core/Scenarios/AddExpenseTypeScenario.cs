using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotHomeFinancesLib.Core.Services;
using TelegramBotHomeFinancesLib.TelegramBot;

namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal class AddExpenseTypeScenario : IScenario
{
    IUserService _userService;
    IExpenseTypeService _expenseTypeService;

    public AddExpenseTypeScenario(IUserService userService, IExpenseTypeService expenseTypeService)
    {
        _userService = userService ?? throw new ArgumentNullException();
        _expenseTypeService = expenseTypeService ?? throw new ArgumentNullException();
    }

    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.AddExpenseType;
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
                await bot.SendMessage(chat, "Введите название типа расхода:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                context.CurrentStep = "Name";
                break;
            case "Name":
                try
                {
                    var expenseType = await _expenseTypeService.Add(financeUser, userInput, ct);
                    if (expenseType == null)
                    {
                        await bot.SendMessage(
                            chat,
                            $"Тип расхода не создан. Сообщение: {userInput}.)",
                            cancellationToken: ct);
                        break;
                    }

                    context.CurrentStep = "Тип расхода создан.";
                    scenarioResult = ScenarioResult.Completed;
                    await bot.SendMessage(chat, "Тип расхода добавлен.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await bot.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
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
