using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotHomeFinancesLib.Core.Services;
using TelegramBotHomeFinancesLib.DTO;
using TelegramBotHomeFinancesLib.TelegramBot;

namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal class AddExpenseScenario : IScenario
{
    IUserService _userService;
    IExpenseService _expenseService;
    IExpenseTypeService _expenseTypeService;

    public AddExpenseScenario(IUserService userService, IExpenseService expenseService, IExpenseTypeService expenseTypeService)
    {
        _userService = userService ?? throw new ArgumentNullException();
        _expenseService = expenseService ?? throw new ArgumentNullException();
        _expenseTypeService = expenseTypeService ?? throw new ArgumentNullException();
    }

    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.AddExpense;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
    {
        var scenarioResult = await (update switch
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
            case "SelectExpenseTypeForAdd":
                if (expenseTypeCallbackDto.ExpenseTypeId == null)
                    break;

                var expenseType = await _expenseTypeService.Get((Guid)expenseTypeCallbackDto.ExpenseTypeId, ct);
                if (expenseType == null)
                {
                    scenarioResult = ScenarioResult.Completed;
                    await botClient.SendMessage(chat, "Тип расхода не найден.", replyMarkup: replyKeyboardDefault, cancellationToken: ct);
                    break;
                }

                var amount = context.Data.TryGetValue("amount", out var amountObj) && amountObj is decimal amountValue ? amountValue : 0m;
                var expense = await _expenseService.Add(financeUser, expenseType, amount, string.Empty, ct);
                if (expense == null)
                {
                    scenarioResult = ScenarioResult.Completed;
                    await botClient.SendMessage(chat, "Расход не создан.", replyMarkup: replyKeyboardDefault, cancellationToken: ct);
                    break;
                }

                context.CurrentStep = "Расход создан.";
                scenarioResult = ScenarioResult.Completed;
                await botClient.SendMessage(chat, $"Расход добавлен: {amount.ToString("0.##", CultureInfo.InvariantCulture)} — {expenseType.Name}.", replyMarkup: replyKeyboardDefault, cancellationToken: ct);
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
                await botClient.SendMessage(chat, "Введите сумму расхода:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                context.CurrentStep = "Amount";
                break;
            case "Amount":
                var amountText = userInput.Replace(" ", "").Replace(',', '.');
                if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
                {
                    await botClient.SendMessage(chat, "Введите корректную сумму расхода (число больше нуля):", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    break;
                }

                context.Data["amount"] = amount;

                var expenseTypes = await _expenseTypeService.GetAllByUserId(financeUser.FinanceUserId, ct);
                if (!expenseTypes.Any())
                {
                    context.CurrentStep = "Нет типов расхода.";
                    scenarioResult = ScenarioResult.Completed;
                    await botClient.SendMessage(chat, "Нет типов расхода. Сначала добавьте тип расхода через команду /showtypeexpense.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                    break;
                }

                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup();
                foreach (var expenseType in expenseTypes)
                {
                    var expenseTypeCallbackDto = new ExpenseTypeCallbackDto
                    {
                        Action = "SelectExpenseTypeForAdd",
                        ExpenseTypeId = expenseType.ExpenseTypeId
                    };
                    inlineKeyboard.AddNewRow(
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: expenseType.Name, callbackData: expenseTypeCallbackDto.ToString()),
                        });
                }

                context.CurrentStep = "ExpenseType";
                await botClient.SendMessage(chat, "Выберите тип расхода:", replyMarkup: inlineKeyboard, cancellationToken: ct);
                break;
            case "ExpenseType":
                // Ожидается выбор типа расхода через inline-кнопку.
                break;
            default:
                break;
        }

        return scenarioResult;
    }
}
