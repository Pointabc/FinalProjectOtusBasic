using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.Services;

internal class IncomeService : IIncomeService
{
    IIncomeRepository _incomeRepository;

    public IncomeService(IIncomeRepository incomeRepository)
    {
        _incomeRepository = incomeRepository;
    }

    public async Task<Income> Add(FinanceUser user, IncomeType? incomeType, decimal amount, string note, CancellationToken ct)
    {
        var income = new Income
        {
            IncomeId = Guid.NewGuid(),
            User = user,
            IncomeType = incomeType,
            Amount = amount,
            CreatedAt = DateTime.UtcNow,
            Note = note,
        };
        await _incomeRepository.Add(income, ct);

        return income;
    }

    public async Task Delete(Guid incomeId, CancellationToken ct)
    {
        await _incomeRepository.Delete(incomeId, ct);
    }

    public async Task<IReadOnlyList<Income>> Find(Guid userId, Func<Income, bool> predicate, CancellationToken ct)
    {
        return await _incomeRepository.Find(userId, predicate, ct);
    }

    public async Task<Income?> Get(Guid incomeId, CancellationToken ct)
    {
        return await _incomeRepository.Get(incomeId, ct);
    }

    public async Task<IReadOnlyList<Income>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        return await _incomeRepository.GetAllByUserId(userId, ct);
    }
}
