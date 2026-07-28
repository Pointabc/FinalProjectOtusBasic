using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotHomeFinancesLib.Core.Services;
using TelegramBotHomeFinancesLib.DTO;
using TelegramBotHomeFinancesLib.TelegramBot;

namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal class DeleteIncomeTypeScenario : IScenario
{
    IUserService _userService;
    IIncomeTypeService _incomeTypeService;
    /// <summary>
    /// Для хранения Guid типа прихода при подтверждении удаления.
    /// </summary>
    Guid _incomeTypeId = Guid.Empty;

    public DeleteIncomeTypeScenario(IUserService userService, IIncomeTypeService incomeTypeService)
    {
        _userService = userService;
        _incomeTypeService = incomeTypeService;
    }

    public bool CanHandle(ScenarioType scenarioType)
    {
        return scenarioType == ScenarioType.DeleteIncomeType;
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

        var incomeTypeCallbackDto = IncomeTypeCallbackDto.FromString(callbackQuery.Data);
        var chat = UpdateHandler.GetChatFromUpdate(update);
        var replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
        var telegramUser = UpdateHandler.GetUserFromUpdate(update);
        var financeUser = await _userService.GetUser(telegramUser.Id, ct);
        if (financeUser == null)
            return scenarioResult;

        switch (incomeTypeCallbackDto.Action)
        {
            case "SelectList":
                if (incomeTypeCallbackDto.IncomeTypeId == null)
                    break;

                // Для хранения Guid списка (категории) для задач при подтверждении удаления.
                _incomeTypeId = (Guid)incomeTypeCallbackDto.IncomeTypeId;
                // Получить список (категорию) для задач по Id.
                var incomeType = await _incomeTypeService.Get((Guid)incomeTypeCallbackDto.IncomeTypeId, ct);

                #region Inline-клавиатура.

                // Создаем клавиатуру
                InlineKeyboardMarkup inlineKeyboardDeleteApprove = new(
                    new[]
                    {
                            // Первый ряд кнопок.
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "✅Да", callbackData: "yes"),
                                InlineKeyboardButton.WithCallbackData(text: "❌Нет", callbackData: "no"),
                            },
                    });

                #endregion

                // Отправляем сообщение с прикрепленной клавиатурой.
                Message message1 = await botClient.SendMessage(
                    chat,
                    text: $"Подтверждаете удаление списка {incomeType?.Name} и всех его задач",
                    replyMarkup: inlineKeyboardDeleteApprove,
                    cancellationToken: ct
                );

                context.CurrentStep = "Delete";
                scenarioResult = ScenarioResult.Transition;

                return scenarioResult;
            case "deleteincometype":
                // Получить типы приходов.
                var userIncomeTypes = await _incomeTypeService.GetAllByUserId(financeUser.FinanceUserId, ct);

                if (!userIncomeTypes.Any())
                {
                    await botClient.SendMessage(
                    chat,
                    text: "Отсутствуют типы приходов для удаления.",
                    replyMarkup: replyKeyboardDefault,
                    cancellationToken: ct);
                    scenarioResult = ScenarioResult.Completed;
                    break;
                }

                // Создать inline-кнопки для выбора списка (категории) для задачи.
                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup();
                foreach (var userIncomeType in userIncomeTypes)
                {
                    inlineKeyboard.AddNewRow(
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: userIncomeType.Name, callbackData: $"SelectList|{userIncomeType.IncomeTypeId}"),
                        });
                }

                // Отправляем сообщение с прикрепленной клавиатурой.
                Message message = await botClient.SendMessage(
                    chat,
                    text: "Выберите тип прихода для удаления",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: ct
                );

                context.CurrentStep = "Delete";
                scenarioResult = ScenarioResult.Transition;
                break;
            case "yes":
                scenarioResult = ScenarioResult.Completed;
                await _incomeTypeService.Delete(_incomeTypeId, ct);
                await botClient.SendMessage(
                    chat,
                    "Тип прихода успешно удален.",
                    replyMarkup: replyKeyboardDefault,
                    cancellationToken: ct);
                break;
            case "no":
                scenarioResult = ScenarioResult.Completed;
                context.CurrentStep = "Сценарий завершен.";

                await botClient.SendMessage(
                    chat,
                    "Удаление типа прихода отменено.",
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
                // Создать inline-кнопки для выбора списка (категории) для задачи.
                InlineKeyboardMarkup inlineKeyboardForDelete = new InlineKeyboardMarkup();
                // Добавить списки (категории) для задач, если есть в хранилище списков (категорий) для задач.
                var userIncomeTypes = await _incomeTypeService.GetAllByUserId(financeUser.FinanceUserId, ct);
                foreach (var userIncomeType in userIncomeTypes)
                {
                    inlineKeyboardForDelete.AddNewRow(
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: userIncomeType.Name, callbackData: $"SelectIncomeType|{userIncomeType.IncomeTypeId}"),
                        });
                }
                await botClient.SendMessage(chat, "Выберете тип прихода для удаления:", replyMarkup: inlineKeyboardForDelete, cancellationToken: ct);
                context.CurrentStep = "Approve";
                break;
            case "Approve":
                try
                {
                    var incomeTypeForDelete = await _incomeTypeService.Get(_incomeTypeId, ct);

                    #region Inline-клавиатура.

                    // Создаем клавиатуру
                    InlineKeyboardMarkup inlineKeyboardDeleteApprove = new(
                        new[]
                        {
                                // Первый ряд кнопок.
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "✅Да", callbackData: "yes"),
                                    InlineKeyboardButton.WithCallbackData(text: "❌Нет", callbackData: "no"),
                                },
                        });

                    // Отправляем сообщение с прикрепленной клавиатурой.
                    Message message1 = await botClient.SendMessage(
                        chat,
                        text: $"Подтверждаете удаление типа прихода {incomeTypeForDelete?.Name}?",
                        replyMarkup: inlineKeyboardDeleteApprove,
                        cancellationToken: ct
                    );

                    #endregion

                    context.CurrentStep = "Delete";
                }
                catch (Exception ex)
                {
                    // TODO VS Проверить!!!
                    await botClient.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
                    /*switch (currentStep)
                    {
                        case "Name":
                            await botClient.SendMessage(chat, "Введите название типа прихода:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                            break;
                    }*/
                }
                break;
            case "Delete":
                if (update.CallbackQuery == null)
                    break;

                switch (update.CallbackQuery.Data)
                {
                    case "yes":
                        // Получить задачи со списком (категории) для задач пользователя.
                        var incomeTypes = await _incomeTypeService.GetAllByUserId(financeUser.FinanceUserId, ct);
                        var incomeType = incomeTypes.Where(x => x.Name == update.Message.Text).FirstOrDefault(); // TODO VS Проверить, по идее нужен текст на inline-кнопке.
                        //var tasks = await _toDoService.GetByUserIdAndList(toDoUser.UserId, new Guid(), ct); // TODO VS Где взять ToDoList Guid?
                                                                                                            // Удалить эти задачи.
                        //foreach (var task in tasks)
                            //await _toDoService.Delete(task.Id, ct);
                        // Удалить тип прихода пользователя.
                        await _incomeTypeService.Delete(incomeType.IncomeTypeId, ct);
                        break;
                    case "no":
                        await botClient.SendMessage(
                            chat,
                            "Удаление типа прихода отменено.",
                            replyMarkup: _replyKeyboardDefault,
                            cancellationToken: ct);
                        scenarioResult = ScenarioResult.Completed;
                        break;
                    default:
                        break;
                }

                scenarioResult = ScenarioResult.Completed;
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
