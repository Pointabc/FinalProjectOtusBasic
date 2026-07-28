using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotHomeFinancesLib.Core.Entities;
using TelegramBotHomeFinancesLib.Core.Services;
using TelegramBotHomeFinancesLib.TelegramBot;
using TelegramBotHomeFinancesLib.DTO;

namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal class AddIncomeScenario : IScenario
{
    IUserService _userService;
    IIncomeService _incomeService;
    Income _income = new Income();                         // Для работы на следующем этапе сценария.

    public AddIncomeScenario(IUserService userService, IIncomeService incomeService)
    {
        _userService = userService ?? throw new ArgumentNullException();
        _incomeService = incomeService ?? throw new ArgumentNullException();
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
        switch (incomeTypeCallbackDto.Action)
        {
            case "SelectList":
                /*var chat = UpdateHandler.GetChatFromUpdate(update);
                // Тут получить список (категорию) для задач по Id и добавить в создаваемую задачу.
                var list = toDoItemCallbackDto.ToDoItemId != null
                    ? await _toDoListService.Get((Guid)toDoItemCallbackDto.ToDoItemId, ct)
                    : null;

                _toDoItem.List = list;
                _toDoItem.StateChangedAt = DateTime.UtcNow;
                _toDoItem.CreatedAt = DateTime.UtcNow;

                var task = await _toDoService.Add(_toDoItem, ct);
                if (task == null)
                {
                    await botClient.SendMessage(
                        chat,
                        $"Нужно добавить описание задачи: {BotConstants.CommandAddTask} [Описание задачи] или создано слишком много задач.",
                        cancellationToken: ct);
                    break;
                }

                context.CurrentStep = "Сценарий завершен.";
                scenarioResult = ScenarioResult.Completed;
                ReplyKeyboardMarkup _replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
                await botClient.SendMessage(chat, "Задача добавлена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);*/

                return ScenarioResult.Completed;
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
        var user = await _userService.GetUser(telegramUser.Id, ct);
        if (user == null)
            return scenarioResult;

        var chat = UpdateHandler.GetChatFromUpdate(update);
        var currentStep = context.CurrentStep;
        ReplyKeyboardMarkup _replyKeyboard = await UpdateHandler.CreateKeyboardMarkupCancel();
        ReplyKeyboardMarkup _replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
        var userInput = UpdateHandler.GetMessageFromUpdate(update);

        switch (currentStep)
        {
            case null:
                context.Data.Add(user.TelegramUserId.ToString(), user);
                await botClient.SendMessage(chat, "Введите название задачи:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                context.CurrentStep = "Name";
                break;
            case "Name":
                /*try
                {
                    _toDoItem = new ToDoItem
                    {
                        User = toDoUser,
                        Name = userInput,
                        Id = Guid.NewGuid(),
                    };
                    _toDoItem.Name = userInput;
                    context.CurrentStep = "Deadline";
                    await botClient.SendMessage(chat, "Введите срок выполнения (dd.MM.yyyy):", replyMarkup: _replyKeyboard, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
                    switch (currentStep)
                    {
                        case "Name":
                            await botClient.SendMessage(chat, "Введите название задачи:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                            break;
                        default:
                            break;
                    }
                }*/
                break;
            case "Deadline":
                // Проверить формат введенной даты.
                /*string format = "dd.MM.yyyy";
                DateTime deadline;
                DateTime.TryParseExact(userInput, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out deadline);
                if (deadline == DateTime.MinValue)
                {
                    await botClient.SendMessage(chat, "Введите срок выполнения (dd.MM.yyyy):", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    break;
                }

                _toDoItem.Deadline = deadline;
                context.CurrentStep = "List";

                // Создать inline-кнопки для выбора списка (категории) для задачи.
                InlineKeyboardButton withOutList = InlineKeyboardButton.WithCallbackData(
                        text: "📌 Без списка",
                        callbackData: $"SelectList|{string.Empty}");
                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(withOutList);
                // Добавить списки (категории) для задач, если есть в хранилище списков (категорий) для задач.
                var userLists = await _toDoListService.GetUserLists(toDoUser.UserId, ct);
                foreach (var list in userLists)
                {
                    inlineKeyboard.AddNewRow(
                        new[]
                        {
                                    InlineKeyboardButton.WithCallbackData(text: list.Name, callbackData: $"SelectList|{list.Id}"),
                        });
                }

                // Отправляем сообщение с прикрепленной клавиатурой.
                Message message1 = await botClient.SendMessage(
                    chat,
                    text: "Выберите список",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: ct
                );*/
                break;
            case "List":
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
