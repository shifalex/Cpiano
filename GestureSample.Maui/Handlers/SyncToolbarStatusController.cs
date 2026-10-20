using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace GestureSample.Maui.Handlers
{
    internal sealed class SyncToolbarStatusController : IDisposable
    {
        private readonly ContentPage _page;
        private readonly BackgroundSyncService _backgroundSyncService;
        private readonly ToolbarItem _statusToolbarItem;
        private bool _isAttached;

        public SyncToolbarStatusController(ContentPage page, BackgroundSyncService backgroundSyncService)
        {
            _page = page;
            _backgroundSyncService = backgroundSyncService;
            _statusToolbarItem = new ToolbarItem
            {
                Text = "Syncing...",
                Order = ToolbarItemOrder.Primary,
                Priority = 99
            };
        }

        public void Attach()
        {
            if (_isAttached)
                return;

            _isAttached = true;
            _backgroundSyncService.StateChanged += OnBackgroundSyncStateChanged;
            Refresh();
        }

        public void Detach()
        {
            if (!_isAttached)
                return;

            _isAttached = false;
            _backgroundSyncService.StateChanged -= OnBackgroundSyncStateChanged;
            RemoveToolbarItem();
        }

        public void Dispose()
        {
            Detach();
        }

        private void OnBackgroundSyncStateChanged(object? sender, EventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_backgroundSyncService.IsSyncing)
                {
                    _statusToolbarItem.Text = string.IsNullOrWhiteSpace(_backgroundSyncService.StatusText)
                        ? "Syncing..."
                        : _backgroundSyncService.StatusText;

                    if (!_page.ToolbarItems.Contains(_statusToolbarItem))
                        _page.ToolbarItems.Add(_statusToolbarItem);

                    return;
                }

                RemoveToolbarItem();
            });
        }

        private void RemoveToolbarItem()
        {
            if (_page.ToolbarItems.Contains(_statusToolbarItem))
                _page.ToolbarItems.Remove(_statusToolbarItem);
        }
    }
}
