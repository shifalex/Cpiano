using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    public sealed class KeyboardOverlayHost : Grid
    {
        public PianoKeyboardReadOnly Keyboard { get; }
        public AbsoluteLayout Overlay { get; } = new() { ZIndex = 99, InputTransparent = true};


        private Border[] _keyOverlays = Array.Empty<Border>();
        private bool _syncScheduled;

        public KeyboardOverlayHost(PianoKeyboardReadOnly keyboard)
        {
            Keyboard = keyboard;
            Children.Add(Keyboard);
            Children.Add(Overlay);
            Keyboard.SizeChanged += (_, _) => SyncOverlay();
        }

        public void EnsureKeyOverlays()
        {
            if (Keyboard.KeyButtons is null || Keyboard.KeyButtons.Count == 0)
                return;


            if (_keyOverlays.Length == Keyboard.KeyButtons.Count)
                return;


            Overlay.Children.Clear();
            _keyOverlays = new Border[Keyboard.KeyButtons.Count];


            for (int i = 0; i < _keyOverlays.Length; i++)
            {
                _keyOverlays[i] = new Border
                {
                    BackgroundColor = Colors.Transparent,
                    Stroke = Colors.Transparent,
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    InputTransparent = true
                };
                Overlay.Children.Add(_keyOverlays[i]);
            }
        }



        // Visual sync: paint overlays according to a bool[] state
        public void SetOverlayState(bool[] bits)
        {
            EnsureKeyOverlays();
            if (bits is null) return;


            int n = Math.Min(bits.Length, _keyOverlays.Length);
            for (int i = 0; i < n; i++)
            {
                if (bits[i])
                {
                    _keyOverlays[i].BackgroundColor = Colors.Yellow.WithAlpha(0.35f);
                    _keyOverlays[i].Stroke = Colors.Yellow.WithAlpha(0.9f);
                    _keyOverlays[i].StrokeThickness = 2;
                }
                else
                {
                    _keyOverlays[i].BackgroundColor = Colors.Transparent;
                    _keyOverlays[i].Stroke = Colors.Transparent;
                    _keyOverlays[i].StrokeThickness = 0;
                }
            }
        }

        public void SyncOverlay()
        {
            EnsureKeyOverlays();
            if (_keyOverlays.Length == 0) return;


            for (int i = 0; i < _keyOverlays.Length; i++)
            {
                var key = Keyboard.KeyButtons[i];


                var rect = new Rect(
                key.X,
                key.Y,
                key.Width,
                key.Height);


                AbsoluteLayout.SetLayoutBounds(_keyOverlays[i], rect);
                AbsoluteLayout.SetLayoutFlags(_keyOverlays[i], AbsoluteLayoutFlags.None);
            }
        }
        // Animation: move a single “cursor” rect from key A to key B
        public async Task AnimateCursor(int fromIndex, int toIndex, uint ms = 250)
        {
            SyncOverlay();


            var from = AbsoluteLayout.GetLayoutBounds(_keyOverlays[fromIndex]);
            var to = AbsoluteLayout.GetLayoutBounds(_keyOverlays[toIndex]);


            var cursor = new Border
            {
                BackgroundColor = Colors.Yellow.WithAlpha(0.7f),
                Stroke = Colors.Yellow.WithAlpha(0.9f),
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                InputTransparent = true
            };


            AbsoluteLayout.SetLayoutBounds(cursor, from);
            Overlay.Children.Add(cursor);


            await cursor.TranslateTo(
            to.X - from.X,
            to.Y - from.Y,
            ms,
            Easing.CubicInOut);


            Overlay.Children.Remove(cursor);
        }

        
    }
}
