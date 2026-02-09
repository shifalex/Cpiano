using GestureSample.Debugging;
using Microsoft.Maui.Layouts;
using System.Text;

namespace GestureSample.Views;

public class DebugHudView : ContentView
{
    const double ExpandedWidth = 340;
    const double ExpandedHeight = 240;
    const double CollapsedWidth = 70;
    const double CollapsedHeight = 34;

    readonly StringBuilder _buffer = new();
    readonly Editor _editor;
    readonly ScrollView _scroll;

    readonly AbsoluteLayout _root;      // HUD internal layout (not fullscreen)
    readonly Frame _panelFrame;         // expanded panel
    readonly Grid _header;
    readonly Button _dbgBtn;            // collapsed button

    readonly Button _pinBtn;
    readonly Button _clearBtn;
    readonly Button _hideBtn;

    bool _isPinned = true;
    bool _isOpen = true;

    // drag state (moves the HUD itself)
    double _startTx, _startTy;

    public DebugHudView()
    {
        // IMPORTANT: the HUD itself is only as big as we request (no fullscreen blocking)
        WidthRequest = ExpandedWidth;
        HeightRequest = ExpandedHeight;

        _root = new AbsoluteLayout();
        Content = _root;

        // --- log ---
        _editor = new Editor { IsReadOnly = true, FontSize = 12, FontFamily = "Consolas" };
        _scroll = new ScrollView { Content = _editor };

        // --- header ---
        _pinBtn = new Button { Text = "Unpin", FontSize = 12, Padding = new Thickness(8, 2) };
        _pinBtn.Clicked += (_, __) => { _isPinned = !_isPinned; ApplyInteractivity(); };

        _clearBtn = new Button { Text = "Clear", FontSize = 12, Padding = new Thickness(8, 2) };
        _clearBtn.Clicked += (_, __) => { _buffer.Clear(); _editor.Text = ""; };

        _hideBtn = new Button { Text = "Hide", FontSize = 12, Padding = new Thickness(8, 2) };
        _hideBtn.Clicked += (_, __) => SetOpen(false);

        _header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
            Padding = new Thickness(6, 4),
        };
        _header.Add(new Label { Text = "DEBUG", FontSize = 12, VerticalTextAlignment = TextAlignment.Center }, 0, 0);
        _header.Add(_pinBtn, 1, 0);
        _header.Add(_clearBtn, 2, 0);
        _header.Add(_hideBtn, 3, 0);

        // Drag the HUD (only when unpinned) by dragging header
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnHeaderPan;
        _header.GestureRecognizers.Add(pan);

        var inner = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
            }
        };
        inner.Add(_header);
        inner.Add(_scroll);
        Grid.SetRow(_scroll, 1);

        _panelFrame = new Frame
        {
            Padding = 0,
            CornerRadius = 12,
            HasShadow = true,
            Opacity = 0.88,
            Content = inner
        };

        // Expanded panel fills the HUD
        AbsoluteLayout.SetLayoutBounds(_panelFrame, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags(_panelFrame, AbsoluteLayoutFlags.All);
        _root.Children.Add(_panelFrame);

        // --- collapsed DBG button (top-right inside HUD) ---
        _dbgBtn = new Button
        {
            Text = "DBG",
            FontSize = 12,
            Padding = new Thickness(10, 4),
            Opacity = 0.85,
            IsVisible = false
        };
        _dbgBtn.Clicked += (_, __) => SetOpen(true);

        AbsoluteLayout.SetLayoutBounds(_dbgBtn, new Rect(1, 0, CollapsedWidth, CollapsedHeight));
        AbsoluteLayout.SetLayoutFlags(_dbgBtn, AbsoluteLayoutFlags.PositionProportional);
        _dbgBtn.AnchorX = 1;
        _dbgBtn.AnchorY = 0;
        _dbgBtn.Margin = new Thickness(0, 8, 8, 0);
        _root.Children.Add(_dbgBtn);

        SetOpen(true, scrollToBottom: false);
        ApplyInteractivity();

        DevLog.Line += OnLine;
        Unloaded += (_, __) => DevLog.Line -= OnLine;
    }

    void ApplyInteractivity()
    {
        // Header always clickable when open
        _header.InputTransparent = !_isOpen;

        // When pinned: log should not steal touches (keyboard under HUD area stays usable)
        bool logInteractive = _isOpen && !_isPinned;
        _scroll.InputTransparent = !logInteractive;
        _editor.InputTransparent = !logInteractive;

        _pinBtn.Text = _isPinned ? "Unpin" : "Pin";
    }

    void OnHeaderPan(object? sender, PanUpdatedEventArgs e)
    {
        if (!_isOpen || _isPinned) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startTx = TranslationX;
                _startTy = TranslationY;
                break;

            case GestureStatus.Running:
                TranslationX = _startTx + e.TotalX;
                TranslationY = _startTy + e.TotalY;
                break;
        }
    }

    void OnLine(string line)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _buffer.AppendLine(line);

            const int max = 6000;
            if (_buffer.Length > max)
                _buffer.Remove(0, _buffer.Length - max);

            _editor.Text = _buffer.ToString();

            // Always keep newest visible when open
            if (_isOpen)
                await ScrollToBottomAsync(false);
        });
    }

    void SetOpen(bool open, bool scrollToBottom = true)
    {
        _isOpen = open;

        _panelFrame.IsVisible = open;
        _dbgBtn.IsVisible = !open;

        // Shrink/grow the HUD view itself so it doesn't keep a large hitbox
        WidthRequest = open ? ExpandedWidth : CollapsedWidth;
        HeightRequest = open ? ExpandedHeight : CollapsedHeight;

        ApplyInteractivity();

        if (open && scrollToBottom)
            _ = ScrollToBottomAsync(false);
    }

    async Task ScrollToBottomAsync(bool animated)
    {
        await Task.Yield();
        await _scroll.ScrollToAsync(_editor, ScrollToPosition.End, animated);
    }
}
