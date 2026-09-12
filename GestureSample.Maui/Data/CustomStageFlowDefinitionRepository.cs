using GestureSample.Maui.Data.SQLite;

namespace GestureSample.Maui.Data
{
    public class CustomStageFlowDefinitionRepository : BaseRepository<CustomStageFlowDefinition>
    {
        public Task<List<CustomStageFlowDefinition>> GetByUserAsync(Guid? userId)
        {
            if (userId == null)
                return Task.FromResult(new List<CustomStageFlowDefinition>());

            return _database.Table<CustomStageFlowDefinition>()
                .Where(item => item.UserId == userId.Value)
                .OrderBy(item => item.Name)
                .ToListAsync();
        }

        public async Task SaveOrUpdateAsync(CustomStageFlowDefinition flow)
        {
            flow.UpdatedAt = DateTime.Now;
            CustomStageFlowDefinition existing = await GetByIdAsync(flow.Id);
            if (existing == null)
                await SaveAsync(flow);
            else
                await UpdateAsync(flow);
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            CustomStageFlowDefinition existing = await GetByIdAsync(id);
            if (existing != null)
                await DeleteAsync(existing);
        }
    }
}
