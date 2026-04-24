using GestureSample.Maui.Data.SQLite;
using GestureSample.Maui.Models.CustomStages;

namespace GestureSample.Maui.Data
{
    public class CustomStageDefinitionRepository : BaseRepository<CustomStageDefinition>
    {
        public Task<List<CustomStageDefinition>> GetByUserAsync(Guid? userId)
        {
            if (userId == null)
                return Task.FromResult(new List<CustomStageDefinition>());

            return _database.Table<CustomStageDefinition>()
                .Where(item => item.UserId == userId.Value)
                .OrderBy(item => item.Name)
                .ToListAsync();
        }

        public Task<List<CustomStageDefinition>> GetByKindAsync(Guid? userId, CustomStageKind kind)
        {
            if (userId == null)
                return Task.FromResult(new List<CustomStageDefinition>());

            string kindName = kind.ToString();
            return _database.Table<CustomStageDefinition>()
                .Where(item => item.UserId == userId.Value && item.StageKindName == kindName)
                .OrderBy(item => item.Name)
                .ToListAsync();
        }

        public async Task SaveOrUpdateAsync(CustomStageDefinition stage)
        {
            stage.UpdatedAt = DateTime.Now;
            CustomStageDefinition existing = await GetByIdAsync(stage.Id);
            if (existing == null)
                await SaveAsync(stage);
            else
                await UpdateAsync(stage);
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            CustomStageDefinition existing = await GetByIdAsync(id);
            if (existing != null)
                await DeleteAsync(existing);
        }
    }
}
