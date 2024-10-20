#if ANDROID
using AndroidX.AppCompat.Widget;
using Android.Graphics.Drawables;
#endif
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Platform;


namespace GestureSample.Maui
{
    public class CustomEntryHandler : EntryHandler
    {
#if ANDROID
        protected override void ConnectHandler(AppCompatEditText nativeView)
        {
            base.ConnectHandler(nativeView);

            var shape = new GradientDrawable();
            shape.SetCornerRadius(20); // Adjust as needed
            shape.SetStroke(2, Android.Graphics.Color.Black); // Border thickness and color
            shape.SetColor(Android.Graphics.Color.White); // Background color


            // Remove the underline by setting the background to null
            nativeView.Background = shape;
            nativeView.SetPadding(20, 20, 20, 20);
        }
#endif
    }
}
