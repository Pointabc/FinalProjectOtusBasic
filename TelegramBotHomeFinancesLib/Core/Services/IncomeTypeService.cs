using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.Services;

internal class IncomeTypeService : IIncomeTypeService
{
    IIncomeTypeRepository _incomeTypeRepository;

    public IncomeTypeService(IIncomeTypeRepository incomeTypeRepository)
    {
        _incomeTypeRepository = incomeTypeRepository;
    }

    public async Task<IncomeType> Add(FinanceUser user, string name, CancellationToken ct)
    {
        var incomeType = new IncomeType
        {
            IncomeTypeId = Guid.NewGuid(),
            User = user,
            Name = name,
            CreatedAt = DateTime.UtcNow,
        };
        await _incomeTypeRepository.Add(incomeType, ct);

        return incomeType;
    }

    public async Task Delete(Guid incomeTypeId, CancellationToken ct)
    {
        await _incomeTypeRepository.Delete(incomeTypeId, ct);
    }

    public async Task<IReadOnlyList<IncomeType>> Find(Guid userId, Func<IncomeType, bool> predicate, CancellationToken ct)
    {
        return await _incomeTypeRepository.Find(userId, predicate, ct);
    }

    public async Task<IncomeType?> Get(Guid incomeTypeId, CancellationToken ct)
    {
        return await _incomeTypeRepository.Get(incomeTypeId, ct);
    }

    public async Task<IReadOnlyList<IncomeType>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        return await _incomeTypeRepository.GetAllByUserId(userId, ct);
    }
}
