using System.Net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotHomeFinancesLib.Core.Scenarios;
using TelegramBotHomeFinancesLib.Core.Services;
using TelegramBotHomeFinancesLib.Infrastructure.DataAccess;
using TelegramBotHomeFinancesLib.TelegramBot;
using static System.Console;

namespace TelegramBotHomeFinancesLib
{
    public class CoreTBot
    {
        public async Task Start()
        {
            try
            {
                #region WebProxy

                // Добавить прокси.
                string? webProxy = Environment.GetEnvironmentVariable("WebProxyForTelegramBot", EnvironmentVariableTarget.User);

                if (string.IsNullOrWhiteSpace(webProxy))
                {
                    WriteLine("Environment not found. Please set the WebProxyForTelegramBot environment variable.");
                    return;
                }
                var proxy = new WebProxy(webProxy);
                var httpClientHandler = new SocketsHttpHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };

                var httpClient = new HttpClient(httpClientHandler);

                #endregion

                // Get token from environment variable
                string? token = Environment.GetEnvironmentVariable("TelegramBotTokenOTUSBasic", EnvironmentVariableTarget.User);
                if (string.IsNullOrWhiteSpace(token))
                {
                    WriteLine("Bot token not found. Please set the [TelegramBotTokenOTUSBasic] environment variable.");
                    return;
                }

                var cancellationTokenSource = new CancellationTokenSource();
                var ct = cancellationTokenSource.Token;
                var botClient = new TelegramBotClient(token, httpClient, ct);

                #region Создать инфраструктуру для сценариев.

                IEnumerable<IScenario> scenarios = new List<IScenario>();
                var contextRepository = new InMemoryScenarioContextRepository();

                #endregion

                var dataContextFactory = new DataContextFactory();
                dataContextFactory.CreateDataContext();
                var expenseRepository = new SqlExpenseRepository(dataContextFactory);
                var incomeRepository = new SqlIncomeRepository(dataContextFactory);
                var incomeTypeRepository = new SqlIncomeTypeRepository(dataContextFactory);
                var incomeTypeService = new IncomeTypeService(incomeTypeRepository);
                var userRepository = new SqlUserRepository(dataContextFactory);
                var expenseService = new ExpenseService(expenseRepository);
                var incomeService = new IncomeService(incomeRepository);
                var userService = new UserService(userRepository);

                var updateHandler = new UpdateHandler(
                    userService,
                    expenseRepository,
                    incomeRepository,
                    incomeTypeService,
                    incomeService,
                    userRepository,
                    contextRepository,
                    botClient,
                    scenarios);

                #region Создать меню и описание команд.

                // Создаем список команд
                var commands = new List<BotCommand>
                {
                    new BotCommand { Command = "start", Description = "Начать работать с ботом." },
                    new BotCommand { Command = "help", Description = "Вывести команды." },
                    new BotCommand { Command = "info", Description = "Информация от боте." },
                    //new BotCommand { Command = "addtask", Description = "Добавить задчу." },
                    //new BotCommand { Command = "show", Description = "Вывести задачи в работе." },
                    //new BotCommand { Command = "report", Description = "Вывести отчет по задачам." },
                };

                // Устанавливаем команды
                await botClient.SetMyCommands(commands);

                #endregion

                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
                ReceiverOptions receiveroptions = new()
                {
                    //AllowedUpdates = Array.Empty<UpdateType>()    // receive all update types
                    AllowedUpdates = new UpdateType[]
                    {
                        UpdateType.Message,          // для обычных сообщений.
                        UpdateType.CallbackQuery,    // для нажатий на кнопки.
                        UpdateType.EditedMessage,    // для отредактированных сообщений.
                        // ... добавьте другие типы, если они нужны
                    }
                };


                botClient.StartReceiving(
                    updateHandler,
                    receiverOptions: receiveroptions,
                    cancellationToken: ct);
                // Basic bot initialization
                var me = await botClient.GetMe();
                WriteLine($"Bot started: @{me.Username}");
                WriteLine("Нажмите клавишу A для выхода.");

                // Отмена асинхронных операции и остановка приложения при нажатии клавиши A.
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        var key = ReadKey(true);
                        if (key.Key == ConsoleKey.A)
                        {
                            //await backgroundTaskRunner.StopTasks(ct);
                            WriteLine("Фоновые задачи остановлены.");
                            cancellationTokenSource.Cancel();
                            WriteLine("Bot stopping...");
                            break;
                        }
                        else
                        {
                            WriteLine($"Id телеграм бота: {me.Id}.");
                        }
                    }
                });

                try
                {
                    await Task.Delay(Timeout.Infinite, ct); // Устанавливаем бесконечную задержку.
                }
                catch (TaskCanceledException)
                {
                    // Ожидаемое исключение - просто игнорируем
                    WriteLine("Bot stopped.");
                }
            }
            catch (Exception e)
            {
                WriteLine("Произошла непредвиденная ошибка: ");
                WriteLine($"Type of exception: {e.GetType()}");
                WriteLine($"Message: {e.Message}");
                WriteLine($"StackTrace: {e.StackTrace}");
                WriteLine($"InnerException: {e.InnerException}");
            }
        }
    }
}
