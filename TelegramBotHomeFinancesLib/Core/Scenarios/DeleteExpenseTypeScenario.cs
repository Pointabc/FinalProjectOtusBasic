using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotHomeFinancesLib.Core.Services;
using TelegramBotHomeFinancesLib.DTO;
using TelegramBotHomeFinancesLib.TelegramBot;

namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal class DeleteExpenseTypeScenario : IScenario
{
    IUserService _userService;
    IExpenseTypeService _expenseTypeService;

    public DeleteExpenseTypeScenario(IUserService userService, IExpenseTypeService expenseTypeService)
    {
        _userService = userService;
        _expenseTypeService = expenseTypeService;
    }

    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.DeleteExpenseType;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
    {
        var scenarioResult = await(update switch
        {
            { Message: { } message } => OnMessage(bot, update, message, context, ct),
            { CallbackQuery: { } callbackQuery } => OnCallbackQuery(bot, update, callbackQuery, context, ct),
            _ => OnUnknown(update)
        });

        return scenarioResult;
    }

    private async Task<ScenarioResult> OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery, ScenarioContext context, CancellationToken ct)
    {
        var scenarioResult = ScenarioResult.Transition;
        await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct); // Чтобы кнопка не мерцала и другие кнопки реагировали.

        if (callbackQuery.Data == null)
            return scenarioResult;

        var expenseTypeCallbackDto = ExpenseTypeCallbackDto.FromString(callbackQuery.Data);
        var chat = UpdateHandler.GetChatFromUpdate(update);
        var replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
        var telegramUser = UpdateHandler.GetUserFromUpdate(update);
        var financeUser = await _userService.GetUser(telegramUser.Id, ct);
        if (financeUser == null)
            return scenarioResult;

        switch (expenseTypeCallbackDto.Action)
        {
            case "SelectExpenseType":
                if (expenseTypeCallbackDto.ExpenseTypeId == null)
                    break;

                context.Data["expenseTypeId"] = (Guid)expenseTypeCallbackDto.ExpenseTypeId;
                var expenseType = await _expenseTypeService.Get((Guid)expenseTypeCallbackDto.ExpenseTypeId, ct);

                InlineKeyboardMarkup inlineKeyboardDeleteApprove = new(
                    new[]
                    {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "✅Да", callbackData: "yes"),
                                InlineKeyboardButton.WithCallbackData(text: "❌Нет", callbackData: "no"),
                            },
                    });

                Message message1 = await botClient.SendMessage(
                    chat,
                    text: $"Подтверждаете удаление типа расхода {expenseType?.Name}?",
                    replyMarkup: inlineKeyboardDeleteApprove,
                    cancellationToken: ct
                );

                context.CurrentStep = "Delete";
                scenarioResult = ScenarioResult.Transition;

                return scenarioResult;
            case "deleteexpensetype":
                var userExpenseTypes = await _expenseTypeService.GetAllByUserId(financeUser.FinanceUserId, ct);

                if (!userExpenseTypes.Any())
                {
                    await botClient.SendMessage(
                    chat,
                    text: "Отсутствуют типы расходов для удаления.",
                    replyMarkup: replyKeyboardDefault,
                    cancellationToken: ct);
                    scenarioResult = ScenarioResult.Completed;
                    break;
                }

                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup();
                foreach (var userExpenseType in userExpenseTypes)
                {
                    inlineKeyboard.AddNewRow(
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: userExpenseType.Name, callbackData: $"SelectExpenseType|{userExpenseType.ExpenseTypeId}"),
                        });
                }

                Message message = await botClient.SendMessage(
                    chat,
                    text: "Выберите тип расхода для удаления",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: ct
                );

                context.CurrentStep = "Delete";
                scenarioResult = ScenarioResult.Transition;
                break;
            case "yes":
                scenarioResult = ScenarioResult.Completed;
                if (context.Data.TryGetValue("expenseTypeId", out var expenseTypeIdObj) && expenseTypeIdObj is Guid expenseTypeId)
                    await _expenseTypeService.Delete(expenseTypeId, ct);
                await botClient.SendMessage(
                    chat,
                    "Тип расхода успешно удален.",
                    replyMarkup: replyKeyboardDefault,
                    cancellationToken: ct);
                break;
            case "no":
                scenarioResult = ScenarioResult.Completed;
                context.CurrentStep = "Сценарий завершен.";

                await botClient.SendMessage(
                    chat,
                    "Удаление типа расхода отменено.",
                    replyMarkup: replyKeyboardDefault,
                    cancellationToken: ct);
                break;
            default:
                break;
        }

        return scenarioResult;
    }

    private async Task<ScenarioResult> OnUnknown(Update update)
    {
        throw new NotImplementedException();
    }

    private async Task<ScenarioResult> OnMessage(ITelegramBotClient botClient, Update update, Message message, ScenarioContext context, CancellationToken ct)
    {
        var scenarioResult = ScenarioResult.Transition;
        if (update == null)
            return scenarioResult;

        var chat = UpdateHandler.GetChatFromUpdate(update);
        var currentStep = context.CurrentStep;
        ReplyKeyboardMarkup _replyKeyboard = await UpdateHandler.CreateKeyboardMarkupCancel();
        ReplyKeyboardMarkup _replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
        var userInput = UpdateHandler.GetMessageFromUpdate(update);
        var userFromUpdate = UpdateHandler.GetUserFromUpdate(update);
        var financeUser = await _userService.GetUser(userFromUpdate.Id, ct);
        if (financeUser == null)
            return scenarioResult;

        switch (currentStep)
        {
            case null:
                InlineKeyboardMarkup inlineKeyboardForDelete = new InlineKeyboardMarkup();
                var userExpenseTypes = await _expenseTypeService.GetAllByUserId(financeUser.FinanceUserId, ct);
                foreach (var userExpenseType in userExpenseTypes)
                {
                    inlineKeyboardForDelete.AddNewRow(
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: userExpenseType.Name, callbackData: $"SelectExpenseType|{userExpenseType.ExpenseTypeId}"),
                        });
                }
                await botClient.SendMessage(chat, "Выберете тип расхода для удаления:", replyMarkup: inlineKeyboardForDelete, cancellationToken: ct);
                context.CurrentStep = "Approve";
                break;
            case "Approve":
                try
                {
                    var expenseTypeIdForDelete = context.Data.TryGetValue("expenseTypeId", out var idObj) && idObj is Guid id ? id : Guid.Empty;
                    var expenseTypeForDelete = await _expenseTypeService.Get(expenseTypeIdForDelete, ct);

                    InlineKeyboardMarkup inlineKeyboardDeleteApprove = new(
                        new[]
                        {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "✅Да", callbackData: "yes"),
                                    InlineKeyboardButton.WithCallbackData(text: "❌Нет", callbackData: "no"),
                                },
                        });

                    Message message1 = await botClient.SendMessage(
                        chat,
                        text: $"Подтверждаете удаление типа расхода {expenseTypeForDelete?.Name}?",
                        replyMarkup: inlineKeyboardDeleteApprove,
                        cancellationToken: ct
                    );

                    context.CurrentStep = "Delete";
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
                }
                break;
            case "Delete":
                // Ожидается нажатие inline-кнопки (Yes/No) — текст игнорируется.
                break;
            case "Cancel":
                scenarioResult = ScenarioResult.Completed;
                context.CurrentStep = "Сценарий завершен.";
                await botClient.SendMessage(chat, "Операция отменена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                break;
            default:
                break;
        }

        return scenarioResult;
    }
}
