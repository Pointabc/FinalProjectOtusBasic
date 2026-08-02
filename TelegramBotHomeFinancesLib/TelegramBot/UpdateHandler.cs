using System.Collections.Concurrent;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;
using TelegramBotHomeFinancesLib.Core.Scenarios;
using TelegramBotHomeFinancesLib.Core.Services;
using TelegramBotHomeFinancesLib.DTO;
using static LinqToDB.Internal.SqlQuery.SqlPredicate;
using static System.Console;

namespace TelegramBotHomeFinancesLib.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        IUserService _userService;
        IExpenseRepository _expenseRepository;
        IIncomeRepository _incomeRepository;
        //IIncomeTypeRepository _incomeTypeRepository;
        IIncomeTypeService _incomeTypeService;
        IExpenseTypeService _expenseTypeService;
        IIncomeService _incomeService;
        IExpenseService _expenseService;
        IUserRepository _userRepository;
        IScenarioContextRepository _contextRepository;
        ITelegramBotClient _botClient;
        ReplyKeyboardMarkup _replyKeyboard;
        ConcurrentDictionary<ScenarioType, IScenario> _scenarios;

        public UpdateHandler(
            IUserService userService,
            IExpenseRepository expenseRepository,
            IIncomeRepository incomeRepository,
            //IIncomeTypeRepository incomeTypeRepository,
            IIncomeTypeService incomeTypeService,
            IExpenseTypeService expenseTypeService,
            IIncomeService incomeService,
            IExpenseService expenseService,
            IUserRepository userRepository,
            IScenarioContextRepository contextRepository,
            ITelegramBotClient botClient)
        {
            _userService = userService ?? throw new ArgumentNullException();
            _expenseRepository = expenseRepository ?? throw new ArgumentNullException();
            _incomeRepository = incomeRepository ?? throw new ArgumentNullException();
            //_incomeTypeRepository = incomeTypeRepository ?? throw new ArgumentNullException();
            _incomeTypeService = incomeTypeService ?? throw new ArgumentNullException();
            _expenseTypeService = expenseTypeService ?? throw new ArgumentNullException();
            _incomeService = incomeService ?? throw new ArgumentNullException();
            _expenseService = expenseService ?? throw new ArgumentNullException();
            _userRepository = userRepository ?? throw new ArgumentNullException();
            _contextRepository = contextRepository ?? throw new ArgumentNullException();
            _botClient = botClient ?? throw new ArgumentNullException();
            _replyKeyboard = new ReplyKeyboardMarkup() ?? throw new ArgumentNullException();
            _scenarios = new ConcurrentDictionary<ScenarioType, IScenario>();
            RegisterScenarios();
        }

        void RegisterScenarios()
        {
            _scenarios.TryAdd(ScenarioType.AddIncome, new AddIncomeScenario(_userService, _incomeService, _incomeTypeService));
            _scenarios.TryAdd(ScenarioType.AddExpense, new AddExpenseScenario(_userService, _expenseService, _expenseTypeService));
            _scenarios.TryAdd(ScenarioType.AddIncomeType, new AddIncomeTypeScenario(_userService, _incomeTypeService));
            _scenarios.TryAdd(ScenarioType.DeleteIncomeType, new DeleteIncomeTypeScenario(_userService, _incomeTypeService));
            _scenarios.TryAdd(ScenarioType.AddExpenseType, new AddExpenseTypeScenario(_userService, _expenseTypeService));
            _scenarios.TryAdd(ScenarioType.DeleteExpenseType, new DeleteExpenseTypeScenario(_userService, _expenseTypeService));
        }

        /// <summary>
        /// Возвращает сессию/сценарий. Если сессия/сценарий не найден, то выбрасывать исключение.
        /// </summary>
        /// <param name="scenario">Тип сессии/сценария.</param>
        /// <returns>Сессия/сценарий.</returns>
        Task<IScenario> GetScenario(ScenarioType scenarioType, long userId)
        {
            if (_scenarios.TryGetValue(scenarioType, out var scenario))
                return Task.FromResult(scenario);

            throw new NullReferenceException($"Тип сессии/сценария {scenarioType} не найден.");
        }

        async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            if (_botClient == null)
                return;

            var user = GetUserFromUpdate(update);
            var scenario = await GetScenario(context.CurrentScenario, user.Id);

            // Обработка команды отмены сценария.
            var input = update switch
            {
                { Message: { Text: { } text } } => text,
                _ => null
            };
            if (string.Equals(input, Constants.CommandCancel, StringComparison.OrdinalIgnoreCase))
            {
                var chat = GetChatFromUpdate(update);
                var replyKeyboardDefault = await CreateKeyboardMarkupDefault();
                await _botClient.SendMessage(chat, "Операция отменена.", replyMarkup: replyKeyboardDefault, cancellationToken: ct);
                await _contextRepository.ResetContext(user.Id, ct);
                return;
            }

            var scenarioResult = await scenario.HandleMessageAsync(_botClient, context, update, ct);

            if (scenarioResult == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(user.Id, ct);
            }
            else
                await _contextRepository.SetContext(user.Id, context, ct);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await (update switch
            {
                { Message: { } message } => OnMessage(botClient, update, message, ct),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(botClient, update, callbackQuery, ct),
                _ => OnUnknown(update)
            });
        }

        private async Task OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery, CancellationToken ct)
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct); // Чтобы кнопка не мерцала и другие кнопки реагировали.
            // Добавить проверку на то, что пользователь зарегистрирован.
            var user = callbackQuery.From;
            var chat = callbackQuery.Message?.Chat;
            var financeUser = await _userService.GetUser(user.Id, ct);
            if (financeUser == null || chat == null)
                return;

            // Также нужно проверять запущен ли для пользователя сценарий и вызывать ProcessScenario.
            var contextRepository = await _contextRepository.GetContext(user.Id, ct);
            if (contextRepository != null)
            {
                await ProcessScenario(contextRepository, update, ct);
                return;
            }

            #region Обработка inline-кнопок

            if (callbackQuery.Data == null)
                return;

            var incomeTypeCallbackDto = IncomeTypeCallbackDto.FromString(callbackQuery.Data);
            // Обрабатываем нажатие в зависимости от callbackData
            switch (incomeTypeCallbackDto.Action)
            {
                case "addincometype":
                    var newScenarioContext = new ScenarioContext(ScenarioType.AddIncomeType)
                    {
                        UserId = financeUser.TelegramUserId
                    };
                    newScenarioContext.Data.Add(Constants.KeyUserIdName, chat.Id);
                    await ProcessScenario(newScenarioContext, update, ct);
                    break;
                case "deleteincometype":
                    var deleteIncomeTypeScenarioContext = new ScenarioContext(ScenarioType.DeleteIncomeType)
                    {
                        UserId = financeUser.TelegramUserId
                    };

                    deleteIncomeTypeScenarioContext.Data.Add(Constants.KeyUserIdName, chat.Id);
                    await ProcessScenario(deleteIncomeTypeScenarioContext, update, ct);
                    break;
                case "addexpensetype":
                    var newExpenseTypeScenarioContext = new ScenarioContext(ScenarioType.AddExpenseType)
                    {
                        UserId = financeUser.TelegramUserId
                    };
                    newExpenseTypeScenarioContext.Data.Add(Constants.KeyUserIdName, chat.Id);
                    await ProcessScenario(newExpenseTypeScenarioContext, update, ct);
                    break;
                case "deleteexpensetype":
                    var deleteExpenseTypeScenarioContext = new ScenarioContext(ScenarioType.DeleteExpenseType)
                    {
                        UserId = financeUser.TelegramUserId
                    };

                    deleteExpenseTypeScenarioContext.Data.Add(Constants.KeyUserIdName, chat.Id);
                    await ProcessScenario(deleteExpenseTypeScenarioContext, update, ct);
                    break;
                default:
                    break;
            }

            #endregion
        }

        private async Task OnMessage(ITelegramBotClient botClient, Update update, Message message, CancellationToken ct)
        {
            try
            {
                if (update == null || message == null)
                    return;

                // Получить пользователя и чат.
                var user = update.Message?.From;
                var chat = update.Message?.Chat;
                if (user == null || chat == null || string.IsNullOrWhiteSpace(message.Text))
                    return;

                var financeUser = await _userService.GetUser(message.From.Id, ct);
                var username = message.From?.Username;
                var chatId = message.Chat.Id;

                // TODO 15032026 эту информацию нужно в логи писать.
                WriteLine($"Received a message from {username}: {message.Text} : sent at {message.Date.ToLocalTime()}");

                // Проверить, есть ли активный сценарий для пользователя.
                var activeContext = await _contextRepository.GetContext(user.Id, ct);
                if (activeContext != null)
                {
                    await ProcessScenario(activeContext, update, ct);
                    return;
                }

                // Echo received message text
                string responseText = string.Empty;

                #region Обработка команд.

                var isRegistratedUser = await ValidateUserAsync(financeUser, botClient, update, _replyKeyboard, ct);

                // Process commands
                switch (message.Text.ToLower())
                {
                    case Constants.CommandStart:
                        if (financeUser == null)
                            financeUser = await _userService.RegisterUser(message.From.Id, message.From.Username, ct);
                        _replyKeyboard = await CreateKeyboardMarkup(financeUser, botClient, update, ct);
                        await botClient.SendMessage(chat, $"Привет, {financeUser.TelegramUserName}", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        break;
                    case Constants.CommandHelp:
                        responseText = CommandHelp();
                        break;
                    case Constants.CommandInfo:
                        responseText = CommandInfo();
                        break;
                    case Constants.CommandAddIncome:
                        if (!isRegistratedUser)
                            break;

                        #region Запустить сессиею/сценарий пользователя добавления задачи.

                        var incomeScenarioContext = new ScenarioContext(ScenarioType.AddIncome)
                        {
                            UserId = financeUser.TelegramUserId
                        };
                        incomeScenarioContext.Data.Add(Constants.KeyUserIdName, chat.Id);
                        await ProcessScenario(incomeScenarioContext, update, ct);

                        #endregion

                        break;
                    case Constants.CommandAddExpense:
                        if (!isRegistratedUser)
                            break;

                        #region Запустить сценарий пользователя добавления расхода.

                        var expenseScenarioContext = new ScenarioContext(ScenarioType.AddExpense)
                        {
                            UserId = financeUser.TelegramUserId
                        };
                        expenseScenarioContext.Data.Add(Constants.KeyUserIdName, chat.Id);
                        await ProcessScenario(expenseScenarioContext, update, ct);

                        #endregion

                        break;
                    case Constants.CommandShowTypeIncome:
                        if (!isRegistratedUser)
                            break;

                        #region Запустить сессиею/сценарий пользователя добавления задачи.

                        /*var incomeTypeScenarioContext = new ScenarioContext(ScenarioType.AddIncomeType)
                        {
                            UserId = financeUser.TelegramUserId
                        };
                        incomeTypeScenarioContext.Data.Add(Constants.KeyUserIdName, chat.Id);
                        var incomeTypeScenario = new AddIncomeTypeScenario(_userService, _incomeTypeService);
                        _scenarios = _scenarios.Append(incomeTypeScenario).ToList();
                        await ProcessScenario(incomeTypeScenarioContext, update, ct);*/

                        #endregion
                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup();
                        // Добавить типы приходов из хранилища.
                        var incomeTypes = await _incomeTypeService.GetAllByUserId(financeUser.FinanceUserId, ct);
                        foreach (var incomeType in incomeTypes)
                        {
                            var incomeTypeCallbackDto = IncomeTypeCallbackDto.FromString($"show|{incomeType.IncomeTypeId}");
                            inlineKeyboard.AddNewRow(
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: incomeType.Name, callbackData: incomeTypeCallbackDto.ToString()),
                                });
                        }
                        // Кнопки Добавить и Удалить.
                        InlineKeyboardButton[] addDelete =
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "🆕 Добавить", callbackData: "addincometype"),
                                InlineKeyboardButton.WithCallbackData(text: "❌ Удалить", callbackData: "deleteincometype"),
                            };
                        inlineKeyboard.AddNewRow(addDelete);

                        // Отправляем сообщение с прикрепленной клавиатурой.
                        Message message1 = await botClient.SendMessage(
                            chat,
                            text: "Типы приходов",
                            replyMarkup: inlineKeyboard,
                            cancellationToken: ct
                        );
                        break;
                    case Constants.CommandShowTypeExpense:
                        if (!isRegistratedUser)
                            break;

                        InlineKeyboardMarkup inlineKeyboardExpense = new InlineKeyboardMarkup();
                        // Добавить типы расходов из хранилища.
                        var expenseTypes = await _expenseTypeService.GetAllByUserId(financeUser.FinanceUserId, ct);
                        foreach (var expenseType in expenseTypes)
                        {
                            var expenseTypeCallbackDto = ExpenseTypeCallbackDto.FromString($"show|{expenseType.ExpenseTypeId}");
                            inlineKeyboardExpense.AddNewRow(
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: expenseType.Name, callbackData: expenseTypeCallbackDto.ToString()),
                                });
                        }
                        // Кнопки Добавить и Удалить.
                        InlineKeyboardButton[] addDeleteExpense =
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "🆕 Добавить", callbackData: "addexpensetype"),
                                InlineKeyboardButton.WithCallbackData(text: "❌ Удалить", callbackData: "deleteexpensetype"),
                            };
                        inlineKeyboardExpense.AddNewRow(addDeleteExpense);

                        // Отправляем сообщение с прикрепленной клавиатурой.
                        Message message2 = await botClient.SendMessage(
                            chat,
                            text: "Типы расходов",
                            replyMarkup: inlineKeyboardExpense,
                            cancellationToken: ct
                        );
                        break;
                    default:
                        WriteLine(Constants.UnknownCommand);
                        responseText = Constants.UnknownCommand;
                        break;
                }

                #endregion

                if (!string.IsNullOrWhiteSpace(responseText))
                {
                    // Send the response
                    Message sentMessage = await botClient.SendMessage(
                        chatId: chatId,
                        text: responseText,
                        cancellationToken: ct);
                }
            }
            catch (Exception e)
            {
                await botClient.SendMessage(update.Message.Chat, e.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
            }
        }

        private async Task OnUnknown(Update update)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Создать разметку клавиатуры по умолчанию (доступны основные действия).
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="botClient">Бот клиент.</param>
        /// <param name="update">Обновления от Telegram.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns></returns>
        private async Task<ReplyKeyboardMarkup> CreateKeyboardMarkup(
            FinanceUser user,
            ITelegramBotClient botClient,
            Update update,
            CancellationToken ct)
        {
            var isValidUser = await ValidateUserAsync(user, botClient, update, _replyKeyboard, ct);
            var buttons = new List<KeyboardButton[]>();

            buttons.Add(new KeyboardButton[] { new KeyboardButton("/start") });
            if (isValidUser)
            {
                buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandAddIncome) });
                buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandAddExpense) });
                buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandShowTypeIncome) });
                buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandShowTypeExpense) });
                buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandViewBalance) });
                // TODO VS Добавить другие кнопки меню.
            }

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        /// <summary>
        /// Проверить пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="botClient">TelegramBot клиент.</param>
        /// <param name="update">Обновленные данные от пользователя.</param>
        /// <param name="replyKeyboard">Клавиатура Telegram бота.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>False - пользователь равен null (отправить сообщение анонимному пользователю о дальнейших действиях), иначе True.</returns>
        static async Task<bool> ValidateUserAsync(
            FinanceUser user,
            ITelegramBotClient botClient,
            Update update,
            ReplyKeyboardMarkup replyKeyboard,
            CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(
                    update.Message.Chat,
                    $"Для начала работы используйте команду {Constants.CommandStart}.", replyMarkup: replyKeyboard,
                    cancellationToken: ct);
                return false;
            }

            return true;
        }

        #region Обработка команд.

        string CommandStart(string username)
        {
            return $"Привет, {username}! Я твой бот. Начнем работать с домашними финансами. Набери {Constants.CommandHelp}, чтобы получить доступные команды.";
        }

        string CommandHelp()
        {
            var responseText = new StringBuilder();
            responseText.AppendLine("Cписок команд:");
            responseText.AppendLine($"{Constants.CommandStart} - Начать работать с ботом.");
            responseText.AppendLine($"{Constants.CommandHelp} - Вывести команды.");
            responseText.AppendLine($"{Constants.CommandInfo} - Вывести информацию о Telegram боте.");
            responseText.AppendLine($"{Constants.CommandAddIncome} - Добавить доход.");
            responseText.AppendLine($"{Constants.CommandAddExpense} - Добавить расход.");
            responseText.AppendLine($"{Constants.CommandViewBalance} - Посмотреть баланс.");
            responseText.AppendLine($"{Constants.CommandShowTypeIncome} - Получить виды доходов.");
            responseText.AppendLine($"{Constants.CommandShowTypeExpense} - Получить виды расходов.");

            return responseText.ToString();
        }

        string CommandInfo()
        {
            return $"Информация о программе.\nВерсия бота 0.0.1. Дата создания {Constants.CreatedDate}";
        }

        public Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            HandleErrorSource source,
            CancellationToken ct)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegran API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            WriteLine(ErrorMessage);

            return Task.CompletedTask;
        }

        #endregion

        /// <summary>
        /// Получить пользователя из объекта обновления.
        /// </summary>
        /// <param name="update">Объект обновления.</param>
        /// <returns>Пользователь.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static User GetUserFromUpdate(Update update)
        {
            if (update.Message != null)
                return update.Message.From;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.From;

            if (update.InlineQuery != null)
                return update.InlineQuery.From;

            if (update.EditedMessage != null)
                return update.EditedMessage.From;

            if (update.ChannelPost != null)
                return update.ChannelPost.From;

            if (update.EditedChannelPost != null)
                return update.EditedChannelPost.From;

            if (update.ChosenInlineResult != null)
                return update.ChosenInlineResult.From;

            throw new InvalidOperationException("Не удалось определить пользователя из update");
        }

        /// <summary>
        /// Получить чат из объекта обновления.
        /// </summary>
        /// <param name="update">Объект обновления.</param>
        /// <returns>Чат.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Chat GetChatFromUpdate(Update update)
        {
            if (update.Message != null)
                return update.Message.Chat;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.Message.Chat;

            if (update.EditedMessage != null)
                return update.EditedMessage.Chat;

            if (update.ChannelPost != null)
                return update.ChannelPost.Chat;

            if (update.EditedChannelPost != null)
                return update.EditedChannelPost.Chat;


            throw new InvalidOperationException("Не удалось определить чат из update");
        }

        /// <summary>
        /// Получить сообщение из объекта обновления.
        /// </summary>
        /// <param name="update">Объект обновления.</param>
        /// <returns>Чат.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetMessageFromUpdate(Update update)
        {
            if (update.Message != null)
                return update.Message.Text;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.Message.Text;

            if (update.EditedMessage != null)
                return update.EditedMessage.Text;

            if (update.ChannelPost != null)
                return update.ChannelPost.Text;

            if (update.EditedChannelPost != null)
                return update.EditedChannelPost.Text;


            throw new InvalidOperationException("Не удалось получить сообщение из update");
        }

        #region Клавиатуры для сценариев и не только.

        /// <summary>
        /// Создать клавиатуру по умолчанию.
        /// </summary>
        /// <returns>Клавиатура по умолчанию.</returns>
        public static async Task<ReplyKeyboardMarkup> CreateKeyboardMarkupDefault()
        {
            var buttons = new List<KeyboardButton[]>();
            buttons.Add(new KeyboardButton[] { new KeyboardButton("/start") });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandAddIncome) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandAddExpense) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandShowTypeIncome) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandShowTypeExpense) });
            //buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandReport) });

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        /// <summary>
        /// Создать клавиатуру во время обработки сценариев.
        /// </summary>
        /// <returns>Клавиатура.</returns>
        public static async Task<ReplyKeyboardMarkup> CreateKeyboardMarkupCancel()
        {
            var buttons = new List<KeyboardButton[]>();
            buttons.Add(new KeyboardButton[] { new KeyboardButton(Constants.CommandCancel) });
            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        #endregion

    }
}
