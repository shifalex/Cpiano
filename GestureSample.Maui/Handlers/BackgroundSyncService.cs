using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SupaBase;
using Microsoft.Maui.ApplicationModel;
using SQLiteUser = GestureSample.Maui.Data.SQLite.User;

namespace GestureSample.Maui.Handlers
{
    internal class BackgroundSyncService
    {
        private readonly GameRepository _gameRepository;

        public BackgroundSyncService(GameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public event EventHandler? StateChanged;

        public bool IsSyncing { get; private set; }
        public string StatusText { get; private set; } = string.Empty;
        public string? LastErrorMessage { get; private set; }
        public DateTime? LastCompletedAt { get; private set; }

        public bool TryStartSync(SQLiteUser? user, bool forceFullResync = false)
        {
            if (user == null || user.Id == Guid.Empty)
            {
                UpdateState(false, "No active user to sync.", "No active user to sync.");
                return false;
            }

            if (IsSyncing)
                return false;

            SQLiteUser snapshot = new()
            {
                Id = user.Id,
                Name = user.Name,
                AvatarUri = user.AvatarUri,
                LastLoginTime = user.LastLoginTime,
                IsTeacher = user.IsTeacher
            };

            _ = RunSyncAsync(snapshot, forceFullResync);
            return true;
        }

        private async Task RunSyncAsync(SQLiteUser user, bool forceFullResync)
        {
            string startText = forceFullResync
                ? $"Re-syncing {user.Name}..."
                : $"Syncing {user.Name}...";

            UpdateState(true, startText, null);

            try
            {
                if (forceFullResync)
                    await _gameRepository.UpdateAllToNotSynced(user.Id);

                await SupabaseService.SyncUserDataAsync(user);
                LastCompletedAt = DateTime.Now;
                UpdateState(false, $"Synced {LastCompletedAt:HH:mm}", null);
            }
            catch (SupabaseService.SyncOfflineException ex)
            {
                LastCompletedAt = DateTime.Now;
                UpdateState(false, "Offline - will sync later", ex.Message);
            }
            catch (Exception ex)
            {
                LastCompletedAt = DateTime.Now;
                UpdateState(false, "Sync failed", ex.Message);
            }
        }

        private void UpdateState(bool isSyncing, string statusText, string? lastErrorMessage)
        {
            IsSyncing = isSyncing;
            StatusText = statusText;
            LastErrorMessage = lastErrorMessage;

            MainThread.BeginInvokeOnMainThread(() => StateChanged?.Invoke(this, EventArgs.Empty));
        }
    }
}
