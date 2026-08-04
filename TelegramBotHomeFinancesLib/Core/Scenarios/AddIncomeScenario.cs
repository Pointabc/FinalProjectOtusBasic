using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotHomeFinancesLib.Core.Entities;
using TelegramBotHomeFinancesLib.Core.Services;
using TelegramBotHomeFinancesLib.DTO;
using TelegramBotHomeFinancesLib.TelegramBot;

namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal class AddIncomeScenario : IScenario
{
    IUserService _userService;
    IIncomeService _incomeService;
    IIncomeTypeService _incomeTypeService;

    public AddIncomeScenario(IUserService userService, IIncomeService incomeService, IIncomeTypeService incomeTypeService)
    {
        _userService = userService ?? throw new ArgumentNullException();
        _incomeService = incomeService ?? throw new ArgumentNullException();
        _incomeTypeService = incomeTypeService ?? throw new ArgumentNullException();
    }

    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.AddIncome;
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

        var incomeTypeCallbackDto = IncomeTypeCallbackDto.FromString(callbackQuery.Data);
        var chat = UpdateHandler.GetChatFromUpdate(update);
        var replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
        var telegramUser = UpdateHandler.GetUserFromUpdate(update);
        var financeUser = await _userService.GetUser(telegramUser.Id, ct);
        if (financeUser == null)
            return scenarioResult;

        switch (incomeTypeCallbackDto.Action)
        {
            case "SelectIncomeType":
                IncomeType? incomeType = null;
                if (incomeTypeCallbackDto.IncomeTypeId != null)
                {
                    incomeType = await _incomeTypeService.Get((Guid)incomeTypeCallbackDto.IncomeTypeId, ct);
                    if (incomeType == null)
                    {
                        scenarioResult = ScenarioResult.Completed;
                        await botClient.SendMessage(chat, "Тип прихода не найден.", replyMarkup: replyKeyboardDefault, cancellationToken: ct);
                        break;
                    }
                }

                var amount = context.Data.TryGetValue("amount", out var amountObj) && amountObj is decimal amountValue ? amountValue : 0m;
                var income = await _incomeService.Add(financeUser, incomeType, amount, string.Empty, ct);
                if (income == null)
                {
                    scenarioResult = ScenarioResult.Completed;
                    await botClient.SendMessage(chat, "Приход не создан.", replyMarkup: replyKeyboardDefault, cancellationToken: ct);
                    break;
                }

                context.CurrentStep = "Приход создан.";
                scenarioResult = ScenarioResult.Completed;
                var typeName = incomeType?.Name ?? "Не определено";
                await botClient.SendMessage(chat, $"Приход добавлен: {amount.ToString("0.##", CultureInfo.InvariantCulture)} — {typeName}.", replyMarkup: replyKeyboardDefault, cancellationToken: ct);
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
                await botClient.SendMessage(chat, "Введите сумму прихода:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                context.CurrentStep = "Amount";
                break;
            case "Amount":
                var amountText = userInput.Replace(" ", "").Replace(',', '.');
                if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
                {
                    await botClient.SendMessage(chat, "Введите корректную сумму прихода (число больше нуля):", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    break;
                }

                context.Data["amount"] = amount;

                var incomeTypes = await _incomeTypeService.GetAllByUserId(financeUser.FinanceUserId, ct);

                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup();
                inlineKeyboard.AddNewRow(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Не определено", callbackData: new IncomeTypeCallbackDto { Action = "SelectIncomeType", IncomeTypeId = null }.ToString()),
                    });
                foreach (var incomeType in incomeTypes)
                {
                    var incomeTypeCallbackDto = new IncomeTypeCallbackDto
                    {
                        Action = "SelectIncomeType",
                        IncomeTypeId = incomeType.IncomeTypeId
                    };
                    inlineKeyboard.AddNewRow(
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: incomeType.Name, callbackData: incomeTypeCallbackDto.ToString()),
                        });
                }

                context.CurrentStep = "IncomeType";
                await botClient.SendMessage(chat, "Выберите тип прихода:", replyMarkup: inlineKeyboard, cancellationToken: ct);
                break;
            case "IncomeType":
                // Ожидается выбор типа прихода через inline-кнопку.
                break;
            default:
                break;
        }

        return scenarioResult;
    }
}
