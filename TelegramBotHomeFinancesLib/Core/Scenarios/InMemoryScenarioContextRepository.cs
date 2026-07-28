using System.Collections.Concurrent;

namespace TelegramBotHomeFinancesLib.Core.Scenarios;

internal class InMemoryScenarioContextRepository : IScenarioContextRepository
{
    ConcurrentDictionary<long, ScenarioContext> _contextRepository = new ConcurrentDictionary<long, ScenarioContext>();

    public async Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
    {
        ScenarioContext? scenario = null;
        _contextRepository.TryGetValue(userId, out scenario);

        return scenario;
    }

    public async Task<IReadOnlyList<ScenarioContext>> GetContexts(CancellationToken ct)
    {
        return _contextRepository.Values.ToList();
    }

    public async Task ResetContext(long userId, CancellationToken ct)
    {
        ScenarioContext scenario = null;
        _contextRepository.Remove(userId, out scenario);
    }

    public async Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
    {
        _contextRepository[userId] = context;
    }
}
