using LinqToDB;
using LinqToDB.Async;
using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Infrastructure.DataAccess
{
    internal class SqlIncomeTypeRepository : IIncomeTypeRepository
    {
        IDataContextFactory<HomeFinanceContext> _factory;

        public SqlIncomeTypeRepository(IDataContextFactory<HomeFinanceContext> factory)
        {
            _factory = factory;
        }

        public async Task Add(IncomeType incomeType, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var incomeTypeModel = ModelMapper.MapToModel(incomeType);
                await dbContext.InsertAsync(incomeTypeModel, token: ct);
            }
        }

        public async Task Delete(Guid incomeTypeId, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var incomeType = await Get(incomeTypeId, ct);
                if (incomeType == null)
                    return;

                var incomeTypeModel = ModelMapper.MapToModel(incomeType);
                await dbContext.DeleteAsync(incomeTypeModel);
            }
        }

        public async Task<IReadOnlyList<IncomeType>> Find(Guid userId, Func<IncomeType, bool> predicate, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                // Сначала загружаем все данные из БД
                var allItems = await dbContext.IncomeTypes
                    .ToListAsync(ct);

                // Применяем предикат в памяти
                var filteredItems = allItems.Select(ModelMapper.MapFromModel).Where(predicate).ToList();

                return filteredItems.AsReadOnly();
            }
        }

        public async Task<IncomeType?> Get(Guid incomeTypeId, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var incomeType = await dbContext.IncomeTypes
                    .Where(i => i.IncomeTypeId == incomeTypeId)
                    .FirstOrDefaultAsync();

                return incomeType != null ? ModelMapper.MapFromModel(incomeType) : null;
            }
        }

        public async Task<IReadOnlyList<IncomeType>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var incomeTypes = await dbContext.IncomeTypes
                    .LoadWith(i => i.User)
                    .Where(i => i.FinanceUserId == userId)
                    .ToListAsync();

                return [.. incomeTypes.Select(ModelMapper.MapFromModel)];
            }
        }
    }
}
